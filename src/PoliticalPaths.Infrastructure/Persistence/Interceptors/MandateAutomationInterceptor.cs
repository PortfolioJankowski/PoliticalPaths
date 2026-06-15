using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PoliticalPaths.Domain.Kadencje;
using PoliticalPaths.Domain.StartyWyborcze;

namespace PoliticalPaths.Infrastructure.Persistence.Interceptors;

public sealed class MandateAutomationInterceptor : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context == null) return result;

        var starts = context.ChangeTracker.Entries<StartWyborczy>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .Select(e => e.Entity)
            .ToList();

        if (!starts.Any())
            return await base.SavingChangesAsync(eventData, result, cancellationToken);

        await EnsureKadencjeExist(context, starts, cancellationToken);
        await EnsureMandatesExist(context, starts, cancellationToken);

        context.ChangeTracker.DetectChanges();

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private async Task EnsureKadencjeExist(DbContext context, List<StartWyborczy> starts, CancellationToken ct)
    {
        var listaIds = starts
            .Where(s => s.ListaId.HasValue)
            .Select(s => s.ListaId!.Value)
            .Distinct()
            .ToList();

        var electionIds = await context.Set<PoliticalPaths.Domain.Wybory.ListaWyborcza>()
            .Where(l => listaIds.Contains(l.Id))
            .Select(l => l.WyboryId)
            .Distinct()
            .ToListAsync(ct);

        var existingKadencje = await context.Set<Kadencja>()
            .Where(k => electionIds.Contains(k.FoundingElectionId))
            .ToDictionaryAsync(k => k.FoundingElectionId, ct);

        foreach (var electionId in electionIds)
        {
            if (existingKadencje.ContainsKey(electionId))
                continue;

            var election = await context.Set<PoliticalPaths.Domain.Wybory.Wybory>()
                .FindAsync([electionId], ct);

            if (election == null)
                continue;

            var rodzaj = await context.Set<PoliticalPaths.Domain.Wybory.RodzajeWyborow>()
                .FindAsync([election.RodzajWyborowId], ct);

            var kadencja = new Kadencja
            {
                Id = Guid.NewGuid(),
                FoundingElectionId = electionId,
                Nazwa = $"{(rodzaj?.Nazwa ?? "Kadencja")} ({election.DataWyborow.Year})",
                DataRozpoczecia = election.DataWyborow
            };

            context.Set<Kadencja>().Add(kadencja);
            existingKadencje[electionId] = kadencja;
        }
    }

    private async Task EnsureMandatesExist(DbContext context, List<StartWyborczy> starts, CancellationToken ct)
    {
        var wynikiIds = starts.Select(s => s.WynikiId).Distinct().ToList();

        var mandateWinningResults = await context.Set<WynikiWyborow>()
            .Where(w => wynikiIds.Contains(w.Id) && w.CzyMandat)
            .Select(w => w.Id)
            .ToListAsync(ct);

        if (!mandateWinningResults.Any())
            return;

        var listaIds = starts
            .Where(s => s.ListaId.HasValue)
            .Select(s => s.ListaId!.Value)
            .Distinct()
            .ToList();

        var listas = await context.Set<PoliticalPaths.Domain.Wybory.ListaWyborcza>()
            .Where(l => listaIds.Contains(l.Id))
            .ToDictionaryAsync(l => l.Id, ct);

        var electionIds = listas.Values
            .Select(l => l.WyboryId)
            .Distinct()
            .ToList();

        var kadencje = await context.Set<Kadencja>()
            .Where(k => electionIds.Contains(k.FoundingElectionId))
            .ToDictionaryAsync(k => k.FoundingElectionId, ct);

        foreach (var start in starts)
        {
            if (!mandateWinningResults.Contains(start.WynikiId))
                continue;

            if (!start.ListaId.HasValue)
                continue;

            if (!listas.TryGetValue(start.ListaId.Value, out var lista))
                continue;

            if (!kadencje.TryGetValue(lista.WyboryId, out var kadencja))
                continue;

            var exists = await context.Set<Mandat>()
                .AnyAsync(m =>
                    m.PolitykId == start.PolitykId &&
                    m.KadencjaId == kadencja.Id, ct);

            if (exists)
                continue;

            var inTracker = context.ChangeTracker.Entries<Mandat>()
                .Any(e =>
                    e.Entity.PolitykId == start.PolitykId &&
                    e.Entity.KadencjaId == kadencja.Id);

            if (inTracker)
                continue;

            context.Set<Mandat>().Add(new Mandat
            {
                Id = Guid.NewGuid(),
                PolitykId = start.PolitykId,
                KadencjaId = kadencja.Id,
                DataOd = kadencja.DataRozpoczecia,
                Status = PoliticalPaths.Domain.Enums.StatusMandatu.Aktywny
            });
        }
    }
}
