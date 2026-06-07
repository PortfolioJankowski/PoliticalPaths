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

        val = await db.SlownikWyborow.FirstOrDefaultAsync(s => s.Nazwa == nazwa, ct);
        if (val == null)
        {
            val = new RodzajeWyborow { Id = Guid.NewGuid(), Nazwa = nazwa, Poziom = poziom };
            db.SlownikWyborow.Add(val);
        }

        await SetToCache(key, val, ct);
        return val;
    }

    public async Task<Wybory> GetOrCreateWyboryAsync(Guid rodzajId, DateOnly data, CancellationToken ct = default)
    {
        var key = $"wybory_{rodzajId}_{data}";
        var val = await GetFromCache<Wybory>(key, ct);
        if (val != null) return val;

        val = await db.MapaWyborow.FirstOrDefaultAsync(w => w.RodzajWyborowId == rodzajId && w.DataWyborow == data, ct);
        if (val == null)
        {
            val = new Wybory
            {
                Id = Guid.NewGuid(),
                RodzajWyborowId = rodzajId,
                DataWyborow = data,
                Ordynacja = Domain.Enums.OrdynacjaWyborcza.Proporcjonalna // Default for Sejm
            };
            db.MapaWyborow.Add(val);
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

    public async Task<KomitetyWyborcze> GetOrCreateKomitetAsync(string nazwa, CancellationToken ct = default)
    {
        var key = $"komitet_{nazwa}";
        var val = await GetFromCache<KomitetyWyborcze>(key, ct);
        if (val != null) return val;

        val = await db.KomitetyWyborcze.FirstOrDefaultAsync(k => k.Nazwa == nazwa, ct);
        if (val == null)
        {
            val = new KomitetyWyborcze { Id = Guid.NewGuid(), Nazwa = nazwa };
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

    public async Task<Formacje> GetOrCreatePartiaAsync(string nazwa, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(nazwa) || nazwa.Equals("bezpartyjny", StringComparison.OrdinalIgnoreCase)) return null!;

        var key = $"partia_{nazwa}";
        var val = await GetFromCache<Formacje>(key, ct);
        if (val != null) return val;

        val = await db.Formacje.FirstOrDefaultAsync(f => f.Nazwa == nazwa, ct);
        if (val == null)
        {
            val = new Formacje { Id = Guid.NewGuid(), Nazwa = nazwa };
            db.Formacje.Add(val);
        }

        await SetToCache(key, val, ct);
        return val;
    }

    public async Task<Politycy> GetOrCreatePolitykAsync(string imie, string nazwisko, CancellationToken ct = default)
    {
        var key = $"polityk_{nazwisko}_{imie}";
        var val = await GetFromCache<Politycy>(key, ct);
        if (val != null) return val;

        val = await db.Politycy.FirstOrDefaultAsync(p => p.Nazwisko == nazwisko && p.Imie == imie, ct);
        if (val == null)
        {
            val = new Politycy { Id = Guid.NewGuid(), Imie = imie, Nazwisko = nazwisko };
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
