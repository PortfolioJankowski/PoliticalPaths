using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PoliticalPaths.Application.Abstractions;
using PoliticalPaths.Application.Abstractions.Persistence;
using PoliticalPaths.Domain.Kadencje;
using PoliticalPaths.Shared.Enums;

namespace PoliticalPaths.Application.Services;

public sealed class MandateGeneratorService(
    IAppDbContext db,
    ILogger<MandateGeneratorService> logger) : IMandateGeneratorService
{
    public async Task GenerateMandatesForElectionAsync(Guid wyboryId, CancellationToken ct = default)
    {
        logger.LogInformation("Generating mandates for election {WyboryId}", wyboryId);

        var election = await db.Wybory
            .FirstOrDefaultAsync(w => w.Id == wyboryId, ct);

        if (election == null)
        {
            logger.LogWarning("Election {WyboryId} not found", wyboryId);
            return;
        }

        // Pobierz zwycięzców (tych, którzy mają CzyMandat = true w WynikiWyborow)
        var winningStarts = await (from s in db.StartyWyborcze
                                   join l in db.ListaWyborcza on s.ListaId equals l.Id
                                   join w in db.WynikiWyborow on s.WynikiId equals w.Id
                                   where l.WyboryId == wyboryId && w.CzyMandat
                                   select s).ToListAsync(ct);

        if (!winningStarts.Any())
        {
            logger.LogInformation("No winners found for election {WyboryId}", wyboryId);
            return;
        }

        // Pobierz istniejące mandaty dla tych wyborów, aby uniknąć duplikatów
        var existingMandates = await db.Mandaty
            .Include(m => m.StartWyborczy)
            .Where(m => m.StartWyborczy.WyboryId == wyboryId)
            .ToDictionaryAsync(m => m.PolitykId, ct);

        int createdCount = 0;

        foreach (var start in winningStarts)
        {
            if (existingMandates.ContainsKey(start.PolitykId))
            {
                continue;
            }

            var mandat = new Mandat
            {
                Id = Guid.NewGuid(),
                PolitykId = start.PolitykId,
                StartWyborczyId = start.Id,
                DataOd = election.DataWyborow, // Wstępna data rozpoczęcia
                Status = StatusMandatu.Aktywny, 
                TypObjecia = TypObjeciaMandatu.WyborBezposredni
            };

            db.Mandaty.Add(mandat);
            
            // Rejestrujemy fakt wyboru jako zdarzenie pierwotne
            db.ZdarzeniaMandatowe.Add(new ZdarzenieMandatowe
            {
                MandatId = mandat.Id,
                PolitykId = start.PolitykId,
                Typ = TypZdarzeniaMandatowego.Wybor,
                DataZdarzenia = election.DataWyborow,
                Opis = "Uzyskanie mandatu w wyniku głosowania"
            });

            createdCount++;
        }

        if (createdCount > 0)
        {
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Successfully created {Count} mandates for election {WyboryId}", createdCount, wyboryId);
        }
    }
}
