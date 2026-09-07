using Microsoft.EntityFrameworkCore;
using PoliticalPaths.Domain.Imports;
using PoliticalPaths.Infrastructure.Persistence;

namespace PoliticalPaths.Dashboard.Services;

public sealed record DashboardSummary(int Files, int Elections, int Politicians, int Mandates);
public sealed record ImportFileListItem(Guid Id, string Name, string Pipeline, ImportFileStatus Status, int Rows, int FailedRows, DateTime? CompletedAt);
public sealed record ElectionListItem(Guid Id, string Type, DateOnly Date, string? Term, int Starts);
public sealed record PoliticianListItem(Guid Id, string Name, int Elections, int Mandates);
public sealed record PoliticianDetails(Guid Id, string Name, DateOnly? BirthDate, IReadOnlyList<ParticipationItem> Participations, IReadOnlyList<MandateEventItem> Events);
public sealed record ParticipationItem(
    DateOnly Date,
    string Election,
    int? District,
    int? ListNumber,
    int? PositionOnList,
    string? Committee,
    string? Party,
    string? SupportingParty,
    int Votes,
    decimal? ListVoteShare,
    bool Mandate);
public sealed record MandateEventItem(DateOnly Date, string Type, string? Description, string? Reference);

public sealed class DashboardQueryService(AppDbContext db)
{
    public Task<DashboardSummary> GetSummaryAsync(CancellationToken ct = default) => GetSummaryCoreAsync(ct);

    private async Task<DashboardSummary> GetSummaryCoreAsync(CancellationToken ct)
    {
        var files = await db.ImportFiles.AsNoTracking().CountAsync(ct);
        var elections = await db.Wybory.AsNoTracking().CountAsync(ct);
        var politicians = await db.Politycy.AsNoTracking().CountAsync(ct);
        var mandates = await db.Mandaty.AsNoTracking().CountAsync(ct);
        return new DashboardSummary(files, elections, politicians, mandates);
    }

    public async Task<List<ImportFileListItem>> GetImportsAsync(CancellationToken ct = default) =>
        await db.ImportFiles.AsNoTracking().OrderByDescending(x => x.RawImportCompletedAt ?? DateTime.MinValue)
            // LogicalNames is stored through a value converter; use the
            // storage path here so the whole projection remains SQL-translatable.
            .Select(x => new ImportFileListItem(x.Id, x.StoragePath, x.ImportBatch.PipelineKey, x.Status, x.TotalRows, x.FailedRows, x.RawImportCompletedAt))
            .Take(200).ToListAsync(ct);

    public async Task<List<ElectionListItem>> GetElectionsAsync(CancellationToken ct = default) =>
        await db.Wybory.AsNoTracking().OrderByDescending(x => x.DataWyborow)
            .Select(x => new ElectionListItem(x.Id, x.Rodzaj.Nazwa, x.DataWyborow, x.Kadencja, x.Czlonkostwa.Count))
            .ToListAsync(ct);

    public async Task<List<PoliticianListItem>> SearchPoliticiansAsync(string? query, int page, int pageSize, CancellationToken ct = default)
    {
        var politicians = db.Politycy.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query))
        {
            query = query.Trim();
            politicians = politicians.Where(x => x.Imie.Contains(query) || x.Nazwisko.Contains(query));
        }

        // Pomelo translates a correlated Distinct().Count() into a derived
        // table which references the outer alias (p.Id). MariaDB rejects
        // that correlation, so load the page first and aggregate its IDs in
        // a separate, SQL-translatable query.
        var pageItems = await politicians.OrderByDescending(x => x.Mandaty.Count).ThenBy(x => x.Nazwisko).ThenBy(x => x.Imie)
            .Select(x => new { x.Id, Name = x.Imie + " " + x.Nazwisko })
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        if (pageItems.Count == 0)
            return [];

        // Keep a List rather than an array: on .NET 10 the optimized
        // Guid[].Contains path can be evaluated as ReadOnlySpan<Guid> by
        // EF's expression funcletizer and fail before SQL generation.
        var ids = pageItems.Select(x => x.Id).ToList();
        var electionCounts = (await db.StartyWyborcze.AsNoTracking()
                .Where(x => ids.Contains(x.PolitykId))
                .Select(x => new { x.PolitykId, x.WyboryId })
                .ToListAsync(ct))
            .GroupBy(x => x.PolitykId)
            .ToDictionary(x => x.Key, x => x.Select(y => y.WyboryId).Distinct().Count());

        var mandateCounts = await db.Mandaty.AsNoTracking()
            .Where(x => ids.Contains(x.PolitykId))
            .GroupBy(x => x.PolitykId)
            .Select(x => new { PolitykId = x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.PolitykId, x => x.Count, ct);

        return pageItems.Select(x => new PoliticianListItem(
            x.Id,
            x.Name,
            electionCounts.GetValueOrDefault(x.Id),
            mandateCounts.GetValueOrDefault(x.Id))).ToList();
    }

    public async Task<PoliticianDetails?> GetPoliticianAsync(Guid id, CancellationToken ct = default)
    {
        var politician = await db.Politycy.AsNoTracking().Where(x => x.Id == id)
            .Select(x => new { x.Id, x.Imie, x.Nazwisko, x.DataUrodzenia }).SingleOrDefaultAsync(ct);
        if (politician is null) return null;

        var rawParticipations = await (
            from start in db.StartyWyborcze.AsNoTracking()
            join election in db.Wybory.AsNoTracking() on start.WyboryId equals election.Id
            join kind in db.RodzajeWyborow.AsNoTracking() on election.RodzajWyborowId equals kind.Id
            join committee in db.KomitetyWyborcze.AsNoTracking() on start.KomitetId equals committee.Id
            join list in db.ListaWyborcza.AsNoTracking() on start.ListaId equals (Guid?)list.Id into lists
            from list in lists.DefaultIfEmpty()
            join district in db.OkregWyborczy.AsNoTracking() on list.OkregId equals district.Id into districts
            from district in districts.DefaultIfEmpty()
            join party in db.Partie.AsNoTracking() on start.PartiaId equals (Guid?)party.Id into parties
            from party in parties.DefaultIfEmpty()
            join supportingParty in db.Partie.AsNoTracking() on start.PopierajacaPartiaId equals (Guid?)supportingParty.Id into supportingParties
            from supportingParty in supportingParties.DefaultIfEmpty()
            where start.PolitykId == id
            orderby election.DataWyborow descending
            select new
            {
                Date = election.DataWyborow,
                Election = kind.Nazwa,
                District = (int?)district.NumerOkregu,
                ListId = (Guid?)list.Id,
                ListNumber = (int?)list.NumerListy,
                PositionOnList = start.NumerNaLiscie,
                Committee = committee.Nazwa,
                Party = party.Nazwa,
                SupportingParty = supportingParty.Nazwa,
                Votes = start.Wyniki.LiczbaGlosow,
                Mandate = start.Wyniki.CzyMandat
            }).ToListAsync(ct);

        var listIds = rawParticipations.Where(x => x.ListId.HasValue).Select(x => x.ListId!.Value).Distinct().ToList();
        var listTotals = listIds.Count == 0
            ? new Dictionary<Guid, int>()
            : await db.StartyWyborcze.AsNoTracking().Where(x => x.ListaId.HasValue && listIds.Contains(x.ListaId.Value))
                .GroupBy(x => x.ListaId!.Value)
                .Select(x => new { ListId = x.Key, Votes = x.Sum(y => y.Wyniki.LiczbaGlosow) })
                .ToDictionaryAsync(x => x.ListId, x => x.Votes, ct);

        var participations = rawParticipations.Select(x => new ParticipationItem(
            x.Date, x.Election, x.District, x.ListNumber, x.PositionOnList, x.Committee, x.Party, x.SupportingParty,
            x.Votes,
            x.ListId is { } listId && listTotals.TryGetValue(listId, out var total) && total > 0
                ? Math.Round(100m * x.Votes / total, 2)
                : null,
            x.Mandate)).ToList();
        var events = await db.ZdarzeniaMandatowe.AsNoTracking().Where(x => x.PolitykId == id).OrderByDescending(x => x.DataZdarzenia)
            .Select(x => new MandateEventItem(x.DataZdarzenia, x.Typ.ToString(), x.Opis, x.DokumentReferencyjny)).ToListAsync(ct);
        return new PoliticianDetails(politician.Id, $"{politician.Imie} {politician.Nazwisko}", politician.DataUrodzenia, participations, events);
    }
}
