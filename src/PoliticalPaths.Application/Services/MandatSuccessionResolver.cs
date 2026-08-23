using Microsoft.EntityFrameworkCore;
using PoliticalPaths.Application.Abstractions.Persistence;
using PoliticalPaths.Application.Abstractions.SejmApiClient;
using PoliticalPaths.Domain.Kadencje;
using PoliticalPaths.Domain.Politycy;
using PoliticalPaths.Shared.Dtos.Sejm;
using PoliticalPaths.Shared.Enums;

namespace PoliticalPaths.Application.Services;

public class MandatSuccessionResolver(IAppDbContext dbContext) : IMandatSuccessionResolver
{
    private const string info = "Uzyskał/a mandat w wyniku sukcesji";
    public async Task ResolveNextMandat(Polityk polityk, ExtendSejmMembersDto extendDto, CancellationToken cancellationToken)
    {
        var kadencja = extendDto.TermNo;
        
        var lista = polityk.StartyWyborcze.Where(s => s.Wybory.Kadencja!.Equals(kadencja))
            .Select(s => s.ListaWyborcza)
            .FirstOrDefault();

        if (lista == null)
        {
            return;
        }
        
        var startujacyZListy = await dbContext.Politycy
            .Include(p => p.StartyWyborcze)
                .ThenInclude(s => s.ListaWyborcza)
            .Include(p => p.StartyWyborcze)
                .ThenInclude(s => s.Wybory)
            .Include(p => p.StartyWyborcze)
                .ThenInclude(s => s.Wyniki)
            .Where(p => p.StartyWyborcze.Any(s =>
                    s.ListaWyborcza.Id == lista.Id &&
                    s.Wybory.Kadencja == kadencja &&
                    !s.Wyniki.CzyMandat)
                && !dbContext.Mandaty.Any(m =>
                    m.PolitykId == p.Id &&
                    m.StartWyborczy.Wybory.Kadencja == kadencja))
            .ToListAsync(cancellationToken);

        var kandydaciWedlugWyniku = startujacyZListy
            .Select(p => new
            {
                Polityk = p,
                Start = p.StartyWyborcze.Single(s =>
                    s.ListaWyborcza.Id == lista.Id &&
                    s.Wybory.Kadencja == kadencja &&
                    !s.Wyniki.CzyMandat)
            })
            .OrderByDescending(x => x.Start.Wyniki.LiczbaGlosow)
            .ThenBy(x => x.Start.NumerNaLiscie)
            .ToList();

        foreach (var kandydatWedlugWyniku in kandydaciWedlugWyniku)
        {
            var kandydat = kandydatWedlugWyniku.Polityk;

            var znaleziony = extendDto
                .SejmMembers
                .FirstOrDefault(m => m.FirstName.Equals(kandydat.Imie, StringComparison.OrdinalIgnoreCase)
                                     && m.LastName.Equals(kandydat.Nazwisko, StringComparison.OrdinalIgnoreCase));

            if (znaleziony is not null)
            {
                //ktoś kto nie miał mandatu w wyborach jest na liście z sejmu
                kandydat.DataUrodzenia = znaleziony.BirthDate;
                kandydat.MiejsceUrodzenia = znaleziony.BirthLocation;
                kandydat.Email = znaleziony.Email;
                kandydatWedlugWyniku.Start.Zawod = znaleziony.Profession;
                kandydatWedlugWyniku.Start.Wyksztalcenie = znaleziony.EducationLevel;

                string opis = info +
                              $"po {polityk.Imie} {polityk.Nazwisko} jako osoba z kolejnego miejsca na liście";
                if (string.IsNullOrWhiteSpace(kandydat.InformacjeDodatkowe))
                {
                    kandydat.InformacjeDodatkowe = opis;
                }
                else
                {
                    var dodatkowe = kandydat.InformacjeDodatkowe;
                    kandydat.InformacjeDodatkowe =  dodatkowe + opis;
                }

                var idMandat = Guid .NewGuid();
                var nowyMandat = new Mandat()
                {
                    Id = idMandat,
                    PolitykId = kandydat.Id,
                    DataOd = extendDto.Term.From.AddDays(1),
                    StartWyborczyId = kandydatWedlugWyniku.Start.Id,
                    Status = StatusMandatu.Aktywny,
                    TypObjecia = TypObjeciaMandatu.Sukcesja,
                };

                var zdarzenieSukcesji = new ZdarzenieMandatowe()
                {
                    DataZdarzenia = extendDto.Term.From.AddDays(1),
                    PolitykId = kandydat.Id,
                    Opis = opis,
                    Typ = TypZdarzeniaMandatowego.Wstąpienie,
                    MandatId = idMandat,
                };
                
                await dbContext.Mandaty.AddAsync(nowyMandat);
                await dbContext.ZdarzeniaMandatowe.AddAsync(zdarzenieSukcesji);
                return;
            }
        }

    }
}
