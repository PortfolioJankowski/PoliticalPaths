using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using PoliticalPaths.Application.Abstractions.Imports;
using PoliticalPaths.Application.Abstractions.Persistence;
using PoliticalPaths.Application.Dtos;
using PoliticalPaths.Application.Services;
using PoliticalPaths.Domain.Enums;
using PoliticalPaths.Domain.Formacje;
using PoliticalPaths.Domain.Politycy;
using PoliticalPaths.Domain.StartyWyborcze;
using PoliticalPaths.Domain.Wybory;

namespace PoliticalPaths.Application.Imports;

public sealed class EntityResolver(IAppDbContext db, IDistributedCache cache) : IEntityResolver
{
    private readonly DistributedCacheEntryOptions _cacheOptions = new DistributedCacheEntryOptions()
        .SetAbsoluteExpiration(TimeSpan.FromMinutes(30));

    private readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
    {
        MaxDepth = 64,
    };

    // Lokalne cache'owanie w ramach cyklu życia Scoped (jedna paczka importu)
    // To znacznie przyspiesza procesowanie tysięcy wierszy unikając serializacji IDistributedCache
    private readonly Dictionary<string, object> _localCache = new();

    private readonly string[] _bezpartyjnyOkreslenia = { "nie należy do partii politycznej" };

    private async Task<T?> GetOrAddAsync<T>(string key, Func<Task<T?>> factory, CancellationToken ct) where T : class
    {
        if (_localCache.TryGetValue(key, out var cached))
            return (T)cached;

        // Próba z IDistributedCache (opcjonalnie, dla wydajności można pominąć jeśli _localCache wystarczy)
        var dtoKey = $"dto_{key}";
        var cachedJson = await cache.GetStringAsync(dtoKey, ct);
        if (cachedJson != null)
        {
            var dto = JsonSerializer.Deserialize<T>(cachedJson, _serializerOptions);
            if (dto != null)
            {
                _localCache[key] = dto;
                return dto;
            }
        }

        var result = await factory();
        if (result != null)
        {
            _localCache[key] = result;
            // Zapisujemy do IDistributedCache tylko DTO lub kluczowe dane, tu upraszczamy
            // Warto rozważyć czy IDistributedCache jest tu w ogóle potrzebny przy masowym imporcie
        }

        return result;
    }

    public async Task<RodzajeWyborow> GetOrCreateSlownikWyborowAsync(string nazwa, PoziomWyborow poziom = PoziomWyborow.Krajowy, CancellationToken ct = default)
    {
        var key = $"rodzaj_{nazwa}";
        if (_localCache.TryGetValue(key, out var cached)) return (RodzajeWyborow)cached;

        var val = db.RodzajeWyborow.Local.FirstOrDefault(s => s.Nazwa == nazwa)
                 ?? await db.RodzajeWyborow.FirstOrDefaultAsync(s => s.Nazwa == nazwa, ct);

        if (val == null)
        {
            val = new RodzajeWyborow { Id = Guid.NewGuid(), Nazwa = nazwa, Poziom = poziom };
            db.RodzajeWyborow.Add(val);
        }

        _localCache[key] = val;
        return val;
    }

    public async Task<Wybory> GetOrCreateWyboryAsync(WyboryDto wyboryDto, CancellationToken ct = default)
    {
        var key = $"wybory_{wyboryDto.RodzajWyborowId}_{wyboryDto.DataWyborow}";
        if (_localCache.TryGetValue(key, out var cached)) return (Wybory)cached;

        var val = db.Wybory.Local.FirstOrDefault(w => w.RodzajWyborowId == wyboryDto.RodzajWyborowId && w.DataWyborow == wyboryDto.DataWyborow)
                 ?? await db.Wybory.FirstOrDefaultAsync(w => w.RodzajWyborowId == wyboryDto.RodzajWyborowId && w.DataWyborow == wyboryDto.DataWyborow, ct);

        if (val == null)
        {
            val = new Wybory
            {
                Id = Guid.NewGuid(),
                RodzajWyborowId = wyboryDto.RodzajWyborowId,
                DataWyborow = wyboryDto.DataWyborow,
                Ordynacja = wyboryDto.Ordynacja,
                CzyPrzedterminowe = wyboryDto.CzyPrzedterminowe,
                DataOgloszenia = wyboryDto.DataOgloszenia,
                Tura = wyboryDto.Tura
            };
            db.Wybory.Add(val);
        }

        _localCache[key] = val;
        return val;
    }

    public async Task<OkregWyborczy> GetOrCreateOkregAsync(int numer, Guid rodzajWyborowId, CancellationToken ct = default)
    {
        var key = $"okreg_{rodzajWyborowId}_{numer}";
        if (_localCache.TryGetValue(key, out var cached)) return (OkregWyborczy)cached;

        var val = db.OkregWyborczy.Local.FirstOrDefault(o => o.NumerOkregu == numer && o.RodzajWyborowId == rodzajWyborowId)
                 ?? await db.OkregWyborczy.FirstOrDefaultAsync(o => o.NumerOkregu == numer && o.RodzajWyborowId == rodzajWyborowId, ct);

        if (val == null)
        {
            val = new OkregWyborczy
            {
                Id = Guid.NewGuid(),
                NumerOkregu = numer,
                RodzajWyborowId = rodzajWyborowId
            };
            db.OkregWyborczy.Add(val);
        }

        _localCache[key] = val;
        return val;
    }

    public async Task GetOrCreateSzczegolyOkregu(SzczegolyOkreguDto dto, CancellationToken ct = default)
    {
        // Szczegóły okręgu rzadko się powtarzają w ramach jednej paczki dla tego samego roku, 
        // ale sprawdzamy Local dla wydajności.
        var szczegoly = db.SzczegolyOkregow.Local.FirstOrDefault(s => s.OkregId == dto.OkregId && s.RokWyborow == dto.RokWyborow)
                       ?? await db.SzczegolyOkregow.FindAsync(new object[] { dto.OkregId, dto.RokWyborow }, ct);

        if (szczegoly == null)
        {
            szczegoly = new SzczegolyOkregu
            {
                OkregId = dto.OkregId,
                RokWyborow = dto.RokWyborow,
                Mieszkancy = dto.Mieszkancy,
                Uprawnieni = dto.Uprawnieni,
                LiczbaKandydatow = dto.LiczbaKandydatow,
                LiczbaList = dto.LiczbaList,
                LiczbaMandatow = dto.LiczbaMandatow
            };
            db.SzczegolyOkregow.Add(szczegoly);
        }
        else
        {
            szczegoly.Mieszkancy = dto.Mieszkancy;
            szczegoly.Uprawnieni = dto.Uprawnieni;
            szczegoly.LiczbaKandydatow = dto.LiczbaKandydatow;
            szczegoly.LiczbaList = dto.LiczbaList;
            szczegoly.LiczbaMandatow = dto.LiczbaMandatow;
        }
    }

    public async Task<KomitetWyborczy> GetOrCreateKomitetAsync(string nazwa, CancellationToken ct = default)
    {
        var key = $"komitet_{nazwa}";
        if (_localCache.TryGetValue(key, out var cached)) return (KomitetWyborczy)cached;

        var val = db.KomitetyWyborcze.Local.FirstOrDefault(k => k.Nazwa == nazwa)
                 ?? await db.KomitetyWyborcze.FirstOrDefaultAsync(k => k.Nazwa == nazwa, ct);

        if (val == null)
        {
            val = new KomitetWyborczy { Id = Guid.NewGuid(), Nazwa = nazwa };
            db.KomitetyWyborcze.Add(val);
        }

        _localCache[key] = val;
        return val;
    }

    public async Task<ListaWyborcza> GetOrCreateListaAsync(Guid okregId, Guid wyboryId, Guid komitetId, int numer, CancellationToken ct = default)
    {
        var key = $"lista_{wyboryId}_{okregId}_{numer}";
        if (_localCache.TryGetValue(key, out var cached)) return (ListaWyborcza)cached;

        var val = db.ListaWyborcza.Local.FirstOrDefault(l => l.OkregId == okregId && l.WyboryId == wyboryId && l.NumerListy == numer)
                 ?? await db.ListaWyborcza.FirstOrDefaultAsync(l => l.OkregId == okregId && l.WyboryId == wyboryId && l.NumerListy == numer, ct);

        if (val == null)
        {
            val = new ListaWyborcza
            {
                Id = Guid.NewGuid(),
                OkregId = okregId,
                WyboryId = wyboryId,
                KomitetWyborczyId = komitetId,
                NumerListy = numer
            };
            db.ListaWyborcza.Add(val);
        }

        _localCache[key] = val;
        return val;
    }

    public async Task<Partia> GetOrCreatePartiaAsync(string nazwa, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(nazwa) || _bezpartyjnyOkreslenia.Contains(nazwa, StringComparer.OrdinalIgnoreCase))
            return null!;

        var key = $"partia_{nazwa}";
        if (_localCache.TryGetValue(key, out var cached)) return (Partia)cached;

        var val = db.Partie.Local.FirstOrDefault(p => p.Nazwa == nazwa)
                 ?? await db.Partie.FirstOrDefaultAsync(p => p.Nazwa == nazwa, ct);

        if (val == null)
        {
            val = new Partia { Id = Guid.NewGuid(), Nazwa = nazwa };
            db.Partie.Add(val);
        }

        _localCache[key] = val;
        return val;
    }

    public WynikiWyborow CreateWynikiAsync(int glosy, bool czyMandat)
    {
        // Wyniki są unikalne dla każdego startu, nie keszujemy ich po kluczu biznesowym
        var wyniki = new WynikiWyborow
        {
            Id = Guid.NewGuid(),
            LiczbaGlosow = glosy,
            CzyMandat = czyMandat
        };

        db.WynikiWyborow.Add(wyniki);
        return wyniki;
    }

    public async Task<Polityk> GetOrCreatePolitykAsync(NamesSurnameDto imionaNazwisko, CancellationToken ct = default)
    {
        var key = $"polityk_{imionaNazwisko.Surname}_{imionaNazwisko.Name}";
        if (_localCache.TryGetValue(key, out var cached)) return (Polityk)cached;

        var val = db.Politycy.Local.FirstOrDefault(p => p.Imie == imionaNazwisko.Name && p.Nazwisko == imionaNazwisko.Surname && p.DrugieImie == imionaNazwisko.SecondName)
                 ?? await db.Politycy.FirstOrDefaultAsync(p => p.Nazwisko == imionaNazwisko.Surname && p.Imie == imionaNazwisko.Name && p.DrugieImie == imionaNazwisko.SecondName, ct);

        if (val == null)
        {
            val = new Polityk { Id = Guid.NewGuid(), Imie = imionaNazwisko.Name, Nazwisko = imionaNazwisko.Surname, DrugieImie = imionaNazwisko.SecondName };
            db.Politycy.Add(val);
        }

        _localCache[key] = val;
        return val;
    }

    public void ClearCache()
    {
        _localCache.Clear();
    }
}
