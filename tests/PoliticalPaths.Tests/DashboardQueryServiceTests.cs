using Microsoft.EntityFrameworkCore;
using PoliticalPaths.Application.Abstractions.Imports.Deserialization;
using PoliticalPaths.Dashboard.Services;
using PoliticalPaths.Domain.Politycy;
using PoliticalPaths.Domain.StartyWyborcze;
using PoliticalPaths.Domain.Wybory;
using PoliticalPaths.Infrastructure.Persistence;
using PoliticalPaths.Shared.Enums;
using Xunit;

namespace PoliticalPaths.Tests;

public sealed class DashboardQueryServiceTests
{
    [Fact]
    public async Task District_history_separates_election_types_and_skips_missing_residents_in_change()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var db = new AppDbContext(options);

        var sejm = new RodzajeWyborow { Id = Guid.NewGuid(), Nazwa = "Sejm", Poziom = PoziomWyborow.Krajowy };
        var senat = new RodzajeWyborow { Id = Guid.NewGuid(), Nazwa = "Senat", Poziom = PoziomWyborow.Krajowy };
        var sejmDistrict = new OkregWyborczy { Id = Guid.NewGuid(), NumerOkregu = 1, RodzajWyborowId = sejm.Id };
        var senatDistrict = new OkregWyborczy { Id = Guid.NewGuid(), NumerOkregu = 1, RodzajWyborowId = senat.Id };
        var elections = new[]
        {
            new Wybory { Id = Guid.NewGuid(), RodzajWyborowId = sejm.Id, Rodzaj = sejm, DataWyborow = new DateOnly(2015, 10, 25) },
            new Wybory { Id = Guid.NewGuid(), RodzajWyborowId = sejm.Id, Rodzaj = sejm, DataWyborow = new DateOnly(2019, 10, 13) },
            new Wybory { Id = Guid.NewGuid(), RodzajWyborowId = sejm.Id, Rodzaj = sejm, DataWyborow = new DateOnly(2023, 10, 15) },
            new Wybory { Id = Guid.NewGuid(), RodzajWyborowId = senat.Id, Rodzaj = senat, DataWyborow = new DateOnly(2023, 10, 15) }
        };
        db.AddRange(sejm, senat, sejmDistrict, senatDistrict);
        db.AddRange(elections);
        db.AddRange(
            DistrictDetails(sejmDistrict, elections[0], 100_000),
            DistrictDetails(sejmDistrict, elections[1], null),
            DistrictDetails(sejmDistrict, elections[2], 120_000),
            DistrictDetails(senatDistrict, elections[3], 80_000));
        await db.SaveChangesAsync();

        var service = new DashboardQueryService(db, new ElectionSourceCatalog(new ImportConfiguration()));
        var overview = await service.GetDistrictHistoryAsync();

        Assert.Equal(2, overview.ElectionTypes.Count);
        var sejmHistory = Assert.Single(overview.ElectionTypes.Single(x => x.ElectionType == "Sejm").Districts).History;
        Assert.Null(sejmHistory.Single(x => x.ElectionDate.Year == 2019).Residents);
        Assert.Equal(20_000, sejmHistory.Single(x => x.ElectionDate.Year == 2023).ResidentsChange);
        Assert.Single(overview.ElectionTypes.Single(x => x.ElectionType == "Senat").Districts);
    }

    private static SzczegolyOkregu DistrictDetails(OkregWyborczy district, Wybory election, int? residents) => new()
    {
        OkregId = district.Id,
        Okreg = district,
        WyboryId = election.Id,
        Wybory = election,
        RokWyborow = election.DataWyborow.Year,
        Mieszkancy = residents,
        Uprawnieni = 70_000,
        LiczbaMandatow = 8,
        LiczbaList = 6,
        LiczbaKandydatow = 80
    };

    [Fact]
    public async Task Politician_details_include_district_and_full_electoral_list()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var db = new AppDbContext(options);

        var kind = new RodzajeWyborow { Id = Guid.NewGuid(), Nazwa = "Sejm", Poziom = PoziomWyborow.Krajowy };
        var election = new Wybory
        {
            Id = Guid.NewGuid(),
            RodzajWyborowId = kind.Id,
            Rodzaj = kind,
            DataWyborow = new DateOnly(2023, 10, 15),
            Kadencja = "X"
        };
        var district = new OkregWyborczy
        {
            Id = Guid.NewGuid(),
            NumerOkregu = 1,
            RodzajWyborowId = kind.Id
        };
        var details = new SzczegolyOkregu
        {
            OkregId = district.Id,
            Okreg = district,
            WyboryId = election.Id,
            Wybory = election,
            RokWyborow = 2023,
            Mieszkancy = 500000,
            Uprawnieni = 400000,
            LiczbaMandatow = 12,
            LiczbaList = 8,
            LiczbaKandydatow = 120
        };
        var committee = new KomitetWyborczy { Id = Guid.NewGuid(), Nazwa = "Komitet testowy" };
        var list = new ListaWyborcza
        {
            Id = Guid.NewGuid(),
            OkregId = district.Id,
            WyboryId = election.Id,
            KomitetWyborczyId = committee.Id,
            NumerListy = 5
        };
        var politician = new Polityk { Id = Guid.NewGuid(), Imie = "Anna", Nazwisko = "Testowa" };
        var colleague = new Polityk { Id = Guid.NewGuid(), Imie = "Jan", Nazwisko = "Listowy" };
        var result = new WynikiWyborow { Id = Guid.NewGuid(), LiczbaGlosow = 12000, CzyMandat = true };
        var colleagueResult = new WynikiWyborow { Id = Guid.NewGuid(), LiczbaGlosow = 6000 };
        var start = new StartWyborczy
        {
            Id = Guid.NewGuid(),
            PolitykId = politician.Id,
            Polityk = politician,
            ListaId = list.Id,
            ListaWyborcza = list,
            NumerNaLiscie = 1,
            KomitetId = committee.Id,
            WynikiId = result.Id,
            Wyniki = result,
            WyboryId = election.Id,
            Wybory = election
        };
        var colleagueStart = new StartWyborczy
        {
            Id = Guid.NewGuid(),
            PolitykId = colleague.Id,
            Polityk = colleague,
            ListaId = list.Id,
            ListaWyborcza = list,
            NumerNaLiscie = 2,
            KomitetId = committee.Id,
            WynikiId = colleagueResult.Id,
            Wyniki = colleagueResult,
            WyboryId = election.Id,
            Wybory = election
        };

        db.AddRange(kind, election, district, details, committee, list, politician, colleague,
            result, colleagueResult, start, colleagueStart);
        await db.SaveChangesAsync();

        var service = new DashboardQueryService(db, new ElectionSourceCatalog(new ImportConfiguration()));
        var profile = await service.GetPoliticianAsync(politician.Id);

        var participation = Assert.Single(profile!.Participations);
        Assert.Equal(500000, participation.DistrictResidents);
        Assert.Equal(400000, participation.DistrictEligibleVoters);
        Assert.Equal(2, participation.ListCandidates.Count);
        Assert.Contains(participation.ListCandidates, candidate =>
            candidate.PoliticianId == politician.Id && candidate.IsCurrentPolitician);
    }

}
