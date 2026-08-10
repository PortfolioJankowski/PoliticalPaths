using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PoliticalPaths.Application.Abstractions.Persistence;
using PoliticalPaths.Application.Abstractions.SejmApiClient;
using PoliticalPaths.Domain.Kadencje;
using PoliticalPaths.Domain.Politycy;
using PoliticalPaths.Shared.Dtos.Sejm;
using PoliticalPaths.Shared.Enums;

namespace PoliticalPaths.Infrastructure.Sejm;

internal class SejmDataExtender(IAppDbContext dbContext, ILogger<SejmDataExtender> logger) : ISejmDataExtender
{
    public async Task ExtendDataAsync(ExtendSejmMembersDto extendDto ,CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(extendDto);
        ArgumentNullException.ThrowIfNull(extendDto.SejmMembers);
        ArgumentNullException.ThrowIfNull(extendDto.Term);
        
        var sejmMembers = extendDto.SejmMembers;
        
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
                s.Wybory.Kadencja == extendDto.TermNo &&
                s.Wybory.Rodzaj.Nazwa.StartsWith("Sejm") &&
                s.Wyniki.CzyMandat
            ))
            .Where(p => surnames.Contains(p.Nazwisko))
            .ToListAsync(cancellationToken);
        
        foreach (var politician in politicians)
        {
            logger.LogDebug($"[SejmDataExtender]: Trying to extend {politician.Imie} {politician.Nazwisko}");
            var choosenMembers = sejmMembers
                .Where(x => x.LastName.Equals(politician.Nazwisko, StringComparison.OrdinalIgnoreCase)
                            && x.FirstName.Equals(politician.Imie, StringComparison.OrdinalIgnoreCase))
                .ToList();
            
            logger.LogDebug($"[SejmDataExtender]: Found {choosenMembers?.Count ?? 0} choosen members");

            await AdjustData(politician, choosenMembers, dbContext, extendDto.TermNo);
        }
        
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task AdjustData(Polityk politician, List<SejmMemberDto> choosenMembers, IAppDbContext dbContext, string kadencja)
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

        var startDlaKonkretnejKadencji = politician.StartyWyborcze.First(s => s.Wybory.Kadencja == kadencja);
        startDlaKonkretnejKadencji.Wybory.Kadencja = kadencja;
        startDlaKonkretnejKadencji.Zawod = choosenCandidate.Profession;
        startDlaKonkretnejKadencji.Wyksztalcenie = choosenCandidate.EducationLevel;
        
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