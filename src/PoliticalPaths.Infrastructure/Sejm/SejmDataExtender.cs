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

internal class SejmDataExtender(IAppDbContext dbContext, 
    ILogger<SejmDataExtender> logger,
    IMandatSuccessionResolver successionResolver) : ISejmDataExtender
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
            .Include(p => p.StartyWyborcze)
                .ThenInclude(s => s.ListaWyborcza)
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

            bool shouldFindNewMember = await AdjustData(
                politician,
                choosenMembers,
                extendDto.TermNo,
                cancellationToken);
            if (shouldFindNewMember)
            {
                await successionResolver.ResolveNextMandat(politician, extendDto, cancellationToken);
            }
        }
        
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<bool> AdjustData(
        Polityk politician,
        List<SejmMemberDto> choosenMembers,
        string kadencja,
        CancellationToken cancellationToken)
    {
        if (choosenMembers.Count == 0)
        {
            return false;
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
            return false;
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
            var startWyborczyId = politician.StartyWyborcze
                .Where(s => s.Wybory.Kadencja == kadencja)
                .Select(s => s.Id)
                .First();

            var mandat = await dbContext.Mandaty
                .FirstOrDefaultAsync(
                    m => m.PolitykId == politician.Id && m.StartWyborczyId == startWyborczyId,
                    cancellationToken);

            if (mandat == null)
            {
                logger.LogWarning($"[SejmDataExtender] Nie znaleziono mandatu, dla polityka! {politician.Id}");
                return false;
            }

            var typZdarzenia = choosenCandidate.InactiveCause switch
            {
                "Zrzeczenie" => TypZdarzeniaMandatowego.Zrzeczenie,
                "Zgon" => TypZdarzeniaMandatowego.Zgon,
                _ => TypZdarzeniaMandatowego.Wygasniecie
            };

            var istniejeTakieZdarzenie = await dbContext.ZdarzeniaMandatowe
                .AnyAsync(
                    z => z.MandatId == mandat.Id && z.Typ == typZdarzenia,
                    cancellationToken);

            mandat.Status = StatusMandatu.Wygasniety;

            if (!istniejeTakieZdarzenie)
            {
                var zdarzenieMandatowe = new ZdarzenieMandatowe
                {
                    Opis = choosenCandidate.InactiveReason,
                    PolitykId = politician.Id,
                    Typ = typZdarzenia,
                    MandatId = mandat.Id,
                    DataZdarzenia = mandat.DataOd.AddDays(1)
                };
                
                dbContext.ZdarzeniaMandatowe.Add(zdarzenieMandatowe);
                logger.LogDebug("[SejmDataExtender] Dodano zdarzenie mandatowe!");
                return true;
            }
        }

        return false;
    }
}
