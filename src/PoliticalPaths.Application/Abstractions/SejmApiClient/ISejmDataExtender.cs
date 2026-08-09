using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PoliticalPaths.Application.Abstractions.Persistence;
using PoliticalPaths.Domain.Enums;
using PoliticalPaths.Domain.Kadencje;
using PoliticalPaths.Domain.Politycy;
using PoliticalPaths.Domain.Wybory;

namespace PoliticalPaths.Application.Abstractions.SejmApiClient;

/// <summary>
/// Extends members info based on SejmAPI response.
/// </summary>
///

//TODO -> dodać do Wyborow pole kadencja - posłuży do dodawania zawodu per start
// spawdzić czy na pewno nie ma więcej typów zdarzeń w api sejmu (oprócz tych 2, bo mapuje zwykłym ifem)
//Sprawdzić serwis do mandatów. Nie wiem czy to powinno być tworzone per import. Może lepiej po prostu robić; IMPORT, Ze startów generować zdarzenia objęcia, robić później Extend. 
//Jeżeli tak to nowy profil w launchsettingsach.
//Data zdarzenia póki co jest niepotrzebna (to jest połaczone z mandatem, a mandat ma datę od. Myślę że datę do można wywalić), dokument referencyjny też useless
public interface ISejmDataExtender
{
    Task ExtendDataAsync(ExtendSejmMembersDto response, int rokKolejnejKadencji, CancellationToken cancellationToken);
}

public class SejmDataExtender(IAppDbContext dbContext, ILogger<SejmDataExtender> logger) : ISejmDataExtender
{
    public async Task ExtendDataAsync(ExtendSejmMembersDto response, int rokKolejnejKadencji ,CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(response.SejmMembers);
        ArgumentNullException.ThrowIfNull(response.Term);
        
        var sejmMembers = response.SejmMembers.SejmMembers;
        
        var surnames = sejmMembers
            .Select(x => x.LastName)
            .Distinct()
            .ToList();

        var politicians = await dbContext.Politycy
            .Include(p => p.StartyWyborcze)
            .ThenInclude(s => s.Wybory)
            .Include(p => p.StartyWyborcze)
            .ThenInclude(s => s.Wyniki)
            .Where(p => p.StartyWyborcze.Any(s =>
                s.Wybory.DataWyborow.Year >= response.Term.From.Year &&
                s.Wybory.DataWyborow.Year < rokKolejnejKadencji))
            .Where(p => p.StartyWyborcze.Any(s => s.Wybory.Rodzaj.Nazwa.StartsWith("Sejm")))
            .Where(p => p.StartyWyborcze.Any(s => s.Wyniki.CzyMandat))
            .Where(x => surnames.Contains(x.Nazwisko))
            .ToListAsync(cancellationToken);
        
        foreach (var politician in politicians)
        {
            logger.LogDebug($"[SejmDataExtender]: Trying to extend {politician.Imie} {politician.Nazwisko}");
            var choosenMembers = sejmMembers
                .Where(x => x.LastName.Equals(politician.Nazwisko, StringComparison.OrdinalIgnoreCase)
                && x.FirstName.Equals(politician.Imie, StringComparison.OrdinalIgnoreCase))
                .ToList();
            
            logger.LogDebug($"[SejmDataExtender]: Found {choosenMembers?.Count ?? 0} choosen members");

            await AdjustData(politician, choosenMembers, dbContext);
        }
        
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task AdjustData(Polityk politician, List<SejmMemberDto> choosenMembers, IAppDbContext dbContext)
    {
        if (choosenMembers.Count == 0)
        {
            return;
        }

        if (choosenMembers.Count > 1)
        {
            var sb = new StringBuilder();
            sb.Append(politician.InformacjeDodatkowe);
            sb.Append(Environment.NewLine);
            sb.Append($"Znaleziono {choosenMembers.Count} kandydatów podczas przypisywania danych z kancelarii Sejmu.");
            foreach (var c in choosenMembers)
            {
                sb.Append(Environment.NewLine);
                sb.Append(
                    $"{c.FirstName} {c.LastName} {c.BirthDate} {c.BirthLocation} {c.Club} {c.Profession} {c.EducationLevel}");
            }
            politician.InformacjeDodatkowe = sb.ToString();
            return;
        }

        var choosenCandidate = choosenMembers[0];
        politician.DataUrodzenia = choosenCandidate.BirthDate;
        politician.MiejsceUrodzenia = choosenCandidate.BirthLocation;
        politician.Email = choosenCandidate.Email;
        logger.LogDebug("[SejmDataExtender]: Rozszerzono dane o polityku {}", politician.Id);

        bool czyZdarzenieMandatowe = !string.IsNullOrWhiteSpace(choosenCandidate.InactiveCause);
        if (czyZdarzenieMandatowe)
        {
            var startWyborczyIds = politician.StartyWyborcze
                .Select(s => s.Id)
                .ToList();

            var mandat = await dbContext.Mandaty
                .FirstOrDefaultAsync(
                    m => m.PolitykId == politician.Id &&
                         startWyborczyIds.Contains(m.StartWyborczyId));

            if (mandat == null)
            {
                logger.LogWarning($"[SejmDataExtender] Nie znaleziono mandatu, dla polityka! {politician.Id}");
                return;
            }
            
            var zdarzenieMandatowe = new ZdarzenieMandatowe
            {
                Opis = choosenCandidate.InactiveReason,
                PolitykId = politician.Id,
                Typ = choosenCandidate.InactiveCause == "Zrzeczenie"
                    ? TypZdarzeniaMandatowego.Zrzeczenie 
                    : TypZdarzeniaMandatowego.Zgon,
                MandatId = mandat.Id
            };

            dbContext.ZdarzeniaMandatowe.Add(zdarzenieMandatowe);
            logger.LogDebug("[SejmDataExtender] Dodano zdarzenie mandatowe!");
        }
    }
}