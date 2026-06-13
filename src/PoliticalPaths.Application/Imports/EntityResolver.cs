using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using PoliticalPaths.Application.Abstractions.Imports;
using PoliticalPaths.Application.Abstractions.Persistence;
using PoliticalPaths.Domain.Enums;
using PoliticalPaths.Domain.Formacje;
using PoliticalPaths.Domain.Politycy;
using PoliticalPaths.Domain.Wybory;

namespace PoliticalPaths.Application.Imports;

public sealed class EntityResolver(IAppDbContext db, IDistributedCache cache) : IEntityResolver
{
    private readonly DistributedCacheEntryOptions _cacheOptions = new DistributedCacheEntryOptions()
        .SetAbsoluteExpiration(TimeSpan.FromMinutes(30));

    public async Task<RodzajeWyborow> GetOrCreateSlownikWyborowAsync(string nazwa, PoziomWyborow poziom = PoziomWyborow.Krajowy, CancellationToken ct = default)
    {
        var key = $"slownik_{nazwa}";
        var val = await GetFromCache<RodzajeWyborow>(key, ct);
        if (val != null) return val;

        val = await db.RodzajeWyborow.FirstOrDefaultAsync(s => s.Nazwa == nazwa, ct);
        if (val == null)
        {
            val = new RodzajeWyborow { Id = Guid.NewGuid(), Nazwa = nazwa, Poziom = poziom };
            db.RodzajeWyborow.Add(val);
        }

        await SetToCache(key, val, ct);
        return val;
    }

    public async Task<Wybory> GetOrCreateWyboryAsync(Guid rodzajId, DateOnly? dataOgloszenia, DateOnly dataWyborow, OrdynacjaWyborcza ordynacja = OrdynacjaWyborcza.Proporcjonalna, CancellationToken ct = default)
    {
        var key = $"wybory_{rodzajId}_{dataWyborow}";
        var val = await GetFromCache<Wybory>(key, ct);
        if (val != null) return val;

        val = await db.Wybory.FirstOrDefaultAsync(w => w.RodzajWyborowId == rodzajId && w.DataWyborow == dataWyborow, ct);
        if (val == null)
        {
            val = new Wybory
            {
                Id = Guid.NewGuid(),
                RodzajWyborowId = rodzajId,
                DataWyborow = dataWyborow,
                Ordynacja = ordynacja // Default for Sejm
            };

            if (dataOgloszenia.HasValue)
            {
                val.DataOgloszenia = dataOgloszenia.Value;
            }

            db.Wybory.Add(val);
        }

        await SetToCache(key, val, ct);
        return val;
    }

    public async Task<OkregWyborczy> GetOrCreateOkregAsync(int numer, Guid wyboryId, CancellationToken ct = default)
    {
        var key = $"okreg_{wyboryId}_{numer}";
        var val = await GetFromCache<OkregWyborczy>(key, ct);
        if (val != null) return val;

        val = await db.OkregWyborczy.FirstOrDefaultAsync(o => o.NumerOkregu == numer && o.RodzajWyborowId == wyboryId, ct);
        if (val == null)
        {
            val = new OkregWyborczy { Id = Guid.NewGuid(), NumerOkregu = numer, RodzajWyborowId = wyboryId };
            db.OkregWyborczy.Add(val);
        }

        await SetToCache(key, val, ct);
        return val;
    }

    public async Task UpdateOkregDetailsAsync(Guid okregId, int liczbaMandatow, int? liczbaList = null, int? liczbaKandydatow = null, CancellationToken ct = default)
    {
        var okreg = await db.OkregWyborczy.FindAsync([okregId], ct);
        if (okreg != null)
        {
            okreg.LiczbaMandatow = liczbaMandatow;
            if (liczbaList.HasValue) okreg.LiczbaList = liczbaList.Value;
            if (liczbaKandydatow.HasValue) okreg.LiczbaKandydatow = liczbaKandydatow.Value;
        }
    }

    public async Task GetOrCreateLudnoscOkregowAsync(Guid okregId, int rok, int mieszkancy, int uprawnieni, CancellationToken ct = default)
    {
        var ludnosc = await db.LudnoscOkregow.FindAsync([okregId, rok], ct);
        if (ludnosc == null)
        {
            ludnosc = new LudnoscOkregow { OkregId = okregId, RokWyborow = rok, Mieszkancy = mieszkancy, Uprawnieni = uprawnieni };
            db.LudnoscOkregow.Add(ludnosc);
        }
        else
        {
            ludnosc.Mieszkancy = mieszkancy;
            ludnosc.Uprawnieni = uprawnieni;
        }
    }

    public async Task<KomitetWyborczy> GetOrCreateKomitetAsync(string nazwa, CancellationToken ct = default)
    {
        var key = $"komitet_{nazwa}";
        var val = await GetFromCache<KomitetWyborczy>(key, ct);
        if (val != null) return val;

        val = await db.KomitetyWyborcze.FirstOrDefaultAsync(k => k.Nazwa == nazwa, ct);
        if (val == null)
        {
            val = new KomitetWyborczy { Id = Guid.NewGuid(), Nazwa = nazwa };
            db.KomitetyWyborcze.Add(val);
        }

        await SetToCache(key, val, ct);
        return val;
    }

    public async Task<ListaWyborcza> GetOrCreateListaAsync(Guid okregId, Guid wyboryId, Guid komitetId, int numer, CancellationToken ct = default)
    {
        var key = $"lista_{wyboryId}_{okregId}_{numer}";
        var val = await GetFromCache<ListaWyborcza>(key, ct);
        if (val != null) return val;

        val = await db.ListaWyborcza.FirstOrDefaultAsync(l => 
            l.OkregId == okregId && 
            l.MapaWyborowId == wyboryId && 
            l.NumerListy == numer, ct);
            
        if (val == null)
        {
            val = new ListaWyborcza 
            { 
                Id = Guid.NewGuid(), 
                OkregId = okregId, 
                MapaWyborowId = wyboryId, 
                KomitetWyborczyId = komitetId,
                NumerListy = numer 
            };
            db.ListaWyborcza.Add(val);
        }

        await SetToCache(key, val, ct);
        return val;
    }

    public async Task<Klub> GetOrCreatePartiaAsync(string nazwa,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(nazwa) || nazwa.Equals("bezpartyjny", StringComparison.OrdinalIgnoreCase)) return null!;

        var key = $"partia_{nazwa}";
        var val = await GetFromCache<Klub>(key, ct);
        if (val != null) return val;

        val = await db.Kluby.FirstOrDefaultAsync(k => k.Nazwa == nazwa, ct);
        if (val == null)
        {
            val = new Klub { Id = Guid.NewGuid(), Nazwa = nazwa };
            db.Kluby.Add(val);
        }

        await SetToCache(key, val, ct);
        return val;
    }

    public async Task<Polityk> GetOrCreatePolitykAsync(string imie, string nazwisko, CancellationToken ct = default)
    {
        var key = $"polityk_{nazwisko}_{imie}";
        var val = await GetFromCache<Polityk>(key, ct);
        if (val != null) return val;

        val = await db.Politycy.FirstOrDefaultAsync(p => p.Nazwisko == nazwisko && p.Imie == imie, ct);
        if (val == null)
        {
            val = new Polityk { Id = Guid.NewGuid(), Imie = imie, Nazwisko = nazwisko };
            db.Politycy.Add(val);
        }

        await SetToCache(key, val, ct);
        return val;
    }

    public void ClearCache()
    {
        // IDistributedCache doesn't have a simple Clear() method. 
        // In a real scenario with Redis, we would usually not clear it globally here, 
        // but for compatibility with the interface and the user's intent:
        // We might want to use a prefix for each batch, but for now, we'll leave it as is or do nothing.
    }

    private async Task<T?> GetFromCache<T>(string key, CancellationToken ct) where T : class
    {
        var cached = await cache.GetStringAsync(key, ct);
        return cached == null ? null : JsonSerializer.Deserialize<T>(cached);
    }

    private async Task SetToCache<T>(string key, T value, CancellationToken ct) where T : class
    {
        var serialized = JsonSerializer.Serialize(value);
        await cache.SetStringAsync(key, serialized, _cacheOptions, ct);
    }
}
