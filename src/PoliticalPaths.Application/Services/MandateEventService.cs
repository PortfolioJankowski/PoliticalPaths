using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PoliticalPaths.Application.Abstractions;
using PoliticalPaths.Application.Abstractions.Persistence;
using PoliticalPaths.Domain.Enums;
using PoliticalPaths.Domain.Kadencje;

namespace PoliticalPaths.Application.Services;

public sealed class MandateEventService(
    IAppDbContext db,
    ILogger<MandateEventService> logger) : IMandateEventService
{
    public async Task AddEventAsync(
        Guid mandatId,
        TypZdarzeniaMandatowego typ,
        DateOnly data,
        string? opis = null,
        string? dokument = null,
        CancellationToken ct = default)
    {
        var mandat = await db.Mandaty
            .FirstOrDefaultAsync(m => m.Id == mandatId, ct);

        if (mandat == null)
        {
            logger.LogError("Mandat {MandatId} not found", mandatId);
            throw new Exception($"Mandat {mandatId} nie istnieje.");
        }

        var zdarzenie = new ZdarzenieMandatowe
        {
            MandatId = mandatId,
            PolitykId = mandat.PolitykId,
            Typ = typ,
            DataZdarzenia = data,
            Opis = opis,
            DokumentReferencyjny = dokument
        };

        db.ZdarzeniaMandatowe.Add(zdarzenie);

        // Logika aktualizacji stanu mandatu
        UpdateMandateStatus(mandat, typ, data);

        await db.SaveChangesAsync(ct);
        
        logger.LogInformation("Added event {Typ} to mandate {MandatId}", typ, mandatId);
    }

    private void UpdateMandateStatus(Mandat mandat, TypZdarzeniaMandatowego typ, DateOnly data)
    {
        switch (typ)
        {
            case TypZdarzeniaMandatowego.Objecie:
            case TypZdarzeniaMandatowego.Wstąpienie:
                mandat.Status = StatusMandatu.Aktywny;
                // Jeśli to pierwsze objęcie, możemy ustawić DataOd, 
                // ale w modelu generatora DataOd jest już wstępnie ustawiona na datę wyborów.
                // Można tu skorygować na faktyczną datę ślubowania.
                mandat.DataOd = data; 
                break;

            case TypZdarzeniaMandatowego.Wygasniecie:
            case TypZdarzeniaMandatowego.Zrzeczenie:
            case TypZdarzeniaMandatowego.ObjecieInnejFunkcji:
                mandat.Status = StatusMandatu.Wygasniety;
                mandat.DataDo = data;
                break;

            case TypZdarzeniaMandatowego.KoniecKadencji:
                mandat.Status = StatusMandatu.Zakonczony;
                mandat.DataDo = data;
                break;
        }
    }
}
