using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PoliticalPaths.Application.Abstractions;
using PoliticalPaths.Application.Abstractions.Persistence;
using PoliticalPaths.Domain.StartyWyborcze;

namespace PoliticalPaths.Application.Services;

public sealed class DHondtMandateAllocationService(
    IAppDbContext db,
    ILogger<DHondtMandateAllocationService> logger) : IDHondtMandateAllocationService
{
    public async Task AllocateForElectionAsync(Guid electionId, CancellationToken cancellationToken = default)
    {
        var starts = await db.StartyWyborcze
            .Include(s => s.ListaWyborcza)
            .Include(s => s.Wyniki)
            .Where(s => s.WyboryId == electionId && s.ListaId != null)
            .ToListAsync(cancellationToken);

        var districts = await db.SzczegolyOkregow
            .Where(d => d.WyboryId == electionId)
            .ToDictionaryAsync(d => d.OkregId, cancellationToken);

        if (starts.Count == 0 || districts.Count == 0)
            throw new InvalidOperationException("Cannot allocate mandates without candidates and district data.");

        var committees = await db.KomitetyWyborcze
            .ToDictionaryAsync(c => c.Id, c => c.Nazwa, cancellationToken);

        var nationalVotes = starts
            .GroupBy(s => s.ListaWyborcza.KomitetWyborczyId)
            .ToDictionary(g => g.Key, g => g.Sum(s => s.Wyniki.LiczbaGlosow));
        var allValidVotes = nationalVotes.Values.Sum();

        if (allValidVotes == 0)
            throw new InvalidOperationException("Cannot allocate mandates: total number of valid votes is zero.");

        var eligibleCommittees = nationalVotes
            .Where(x => x.Value >= allValidVotes * GetThreshold(committees[x.Key]))
            .Select(x => x.Key)
            .ToHashSet();

        foreach (var start in starts)
            start.Wyniki.CzyMandat = false;

        var elected = new List<StartWyborczy>();
        foreach (var district in districts)
        {
            var districtStarts = starts.Where(s => s.ListaWyborcza.OkregId == district.Key).ToList();
            var seatCount = district.Value.LiczbaMandatow;

            if (districtStarts.Count == 0)
                throw new InvalidOperationException($"District {district.Key} has no candidate results.");

            var lists = districtStarts
                .GroupBy(s => s.ListaId!.Value)
                .Select(g => new ElectoralList(
                    g.Key,
                    g.First().ListaWyborcza.NumerListy,
                    g.First().ListaWyborcza.KomitetWyborczyId,
                    g.Sum(s => s.Wyniki.LiczbaGlosow),
                    g.ToList()))
                .Where(l => eligibleCommittees.Contains(l.CommitteeId))
                .ToList();

            if (lists.Count == 0)
                throw new InvalidOperationException($"District {district.Key} has no lists eligible for allocation.");

            var allocatedSeats = AllocateSeats(lists, seatCount);
            foreach (var allocation in allocatedSeats)
            {
                var winners = allocation.List.Candidates
                    .OrderByDescending(s => s.Wyniki.LiczbaGlosow)
                    .ThenBy(s => s.NumerNaLiscie ?? int.MaxValue)
                    .Take(allocation.Seats)
                    .ToList();

                if (winners.Count != allocation.Seats)
                    throw new InvalidOperationException($"List {allocation.List.ListNumber} in district {district.Key} has too few candidates.");

                elected.AddRange(winners);
            }
        }

        if (elected.Count != districts.Values.Sum(d => d.LiczbaMandatow))
            throw new InvalidOperationException("The allocated mandate count does not match the district mandate count.");

        foreach (var winner in elected)
            winner.Wyniki.CzyMandat = true;

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Allocated {MandateCount} mandates using d'Hondt for election {ElectionId}.", elected.Count, electionId);
    }

    private static double GetThreshold(string committeeName) => committeeName switch
    {
        var name when name.Contains("Mniejszość Niemiecka", StringComparison.OrdinalIgnoreCase) => 0d,
        var name when name.StartsWith("Koalicyjny Komitet Wyborczy", StringComparison.OrdinalIgnoreCase) => .08d,
        _ => .05d
    };

    private static IReadOnlyList<ListAllocation> AllocateSeats(IReadOnlyList<ElectoralList> lists, int seatCount)
    {
        var quotients = lists
            .SelectMany(list => Enumerable.Range(1, seatCount)
                .Select(divisor => new Quotient(list, divisor)))
            .OrderByDescending(q => (decimal)q.List.Votes / q.Divisor)
            .ThenByDescending(q => q.List.Votes)
            .ThenBy(q => q.List.ListNumber)
            .Take(seatCount);

        return quotients
            .GroupBy(q => q.List)
            .Select(g => new ListAllocation(g.Key, g.Count()))
            .ToList();
    }

    private sealed record ElectoralList(Guid Id, int ListNumber, Guid CommitteeId, int Votes, List<StartWyborczy> Candidates);
    private sealed record Quotient(ElectoralList List, int Divisor);
    private sealed record ListAllocation(ElectoralList List, int Seats);
}
