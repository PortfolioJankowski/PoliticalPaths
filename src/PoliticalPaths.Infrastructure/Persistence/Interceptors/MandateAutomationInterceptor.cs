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

        await HandleElectionResults(context, cancellationToken);
        await HandleMandateEvents(context, cancellationToken);

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private async Task HandleElectionResults(DbContext context, CancellationToken ct)
    {
        var wonResults = context.ChangeTracker.Entries<WynikiWyborow>()
            .Where(e => (e.State == EntityState.Added || e.State == EntityState.Modified) && e.Entity.CzyMandat)
            .Select(e => e.Entity)
            .ToList();

        if (!wonResults.Any()) return;

        var startIds = wonResults.Select(r => r.StartId).ToList();
        var starts = await context.Set<StartWyborczy>()
            .Where(s => startIds.Contains(s.Id))
            .ToListAsync(ct);

        var listaIds = starts.Where(s => s.ListaId.HasValue).Select(s => s.ListaId!.Value).Distinct().ToList();
        var listas = await context.Set<PoliticalPaths.Domain.Wybory.ListaWyborcza>()
            .Where(l => listaIds.Contains(l.Id))
            .ToListAsync(ct);

        var electionIds = listas.Select(l => l.MapaWyborowId).Distinct().ToList();
        var kadencje = await context.Set<Kadencja>()
            .Where(k => electionIds.Contains(k.FoundingElectionId))
            .ToListAsync(ct);

        var electionMap = new Dictionary<Guid, PoliticalPaths.Domain.Wybory.Wybory>();
        var termMap = kadencje.ToDictionary(k => k.FoundingElectionId);

        foreach (var start in starts)
        {
            var lista = listas.FirstOrDefault(l => l.Id == start.ListaId);
            if (lista == null) continue;

            var electionId = lista.MapaWyborowId;
            if (!termMap.TryGetValue(electionId, out var kadencja))
            {
                // Check if already created in this SaveChanges session
                kadencja = context.ChangeTracker.Entries<Kadencja>()
                    .Select(e => e.Entity)
                    .FirstOrDefault(k => k.FoundingElectionId == electionId);

                if (kadencja == null)
                {
                    if (!electionMap.TryGetValue(electionId, out var election))
                    {
                        election = await context.Set<PoliticalPaths.Domain.Wybory.Wybory>().FindAsync([electionId], ct);
                        if (election != null) electionMap[electionId] = election;
                    }

                    if (election != null)
                    {
                        var rodzaj = await context.Set<PoliticalPaths.Domain.Wybory.RodzajeWyborow>().FindAsync([election.RodzajWyborowId], ct);
                        kadencja = new Kadencja
                        {
                            Id = Guid.NewGuid(),
                            FoundingElectionId = electionId,
                            Nazwa = $"{(rodzaj?.Nazwa ?? "Kadencja")} ({election.DataWyborow.Year})",
                            DataRozpoczecia = election.DataWyborow
                        };
                        context.Set<Kadencja>().Add(kadencja);
                    }
                }
                
                if (kadencja != null) termMap[electionId] = kadencja;
            }

            if (kadencja != null)
            {
                var existing = await context.Set<Mandat>()
                    .AnyAsync(m => m.PolitykId == start.PolitykId && m.KadencjaId == kadencja.Id, ct);

                if (!existing)
                {
                    // Also check tracker
                    var inTracker = context.ChangeTracker.Entries<Mandat>()
                        .Any(e => e.Entity.PolitykId == start.PolitykId && e.Entity.KadencjaId == kadencja.Id);

                    if (!inTracker)
                    {
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
        }
    }

    private async Task HandleMandateEvents(DbContext context, CancellationToken ct)
    {
        var entries = context.ChangeTracker.Entries<ZdarzenieMandatowe>()
            .Where(e => e.State == EntityState.Added)
            .ToList();

        foreach (var entry in entries)
        {
            var ev = entry.Entity;
            var mandate = await context.Set<Mandat>().FindAsync([ev.MandatId], ct);
            if (mandate == null) continue;

            switch (ev.Typ)
            {
                case TypZdarzeniaMandatowego.Wygasniecie:
                case TypZdarzeniaMandatowego.Zrzeczenie:
                case TypZdarzeniaMandatowego.ObjecieInnejFunkcji:
                case TypZdarzeniaMandatowego.KoniecKadencji:
                    mandate.DataDo = ev.DataZdarzenia;
                    mandate.Status = PoliticalPaths.Domain.Enums.StatusMandatu.Wygasniety;
                    break;
                case TypZdarzeniaMandatowego.Objecie:
                    mandate.DataOd = ev.DataZdarzenia;
                    mandate.Status = PoliticalPaths.Domain.Enums.StatusMandatu.Aktywny;
                    break;
            }
        }
    }
}
