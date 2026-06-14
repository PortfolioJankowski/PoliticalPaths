using System.Linq;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using PoliticalPaths.Application.Abstractions.Imports;
using PoliticalPaths.Application.Abstractions.Persistence;
using PoliticalPaths.Application.Dtos;
using PoliticalPaths.Domain.Enums;
using PoliticalPaths.Domain.Formacje;
using PoliticalPaths.Domain.Politycy;
using PoliticalPaths.Domain.Wybory;

namespace PoliticalPaths.Application.Imports;

public sealed class EntityResolver(IAppDbContext db, IDistributedCache cache) : IEntityResolver
{
    private readonly DistributedCacheEntryOptions _cacheOptions = new DistributedCacheEntryOptions()
        .SetAbsoluteExpiration(TimeSpan.FromMinutes(30));

    private readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions()
    {
        MaxDepth = 64,
    };

    private async Task<TDto?> GetFromCacheDto<TDto>(string key, CancellationToken ct) where TDto : class
    {
        var cached = await cache.GetStringAsync(key, ct);
        return cached == null ? null : JsonSerializer.Deserialize<TDto>(cached, _serializerOptions);
    }

    private async Task SetToCacheDto<TDto>(string key, TDto value, CancellationToken ct) where TDto : class
    {
        var serialized = JsonSerializer.Serialize(value, options: _serializerOptions);
        await cache.SetStringAsync(key, serialized, _cacheOptions, ct);
    }

    public async Task<RodzajeWyborow> GetOrCreateSlownikWyborowAsync(string nazwa, PoziomWyborow poziom = PoziomWyborow.Krajowy, CancellationToken ct = default)
    {
        var key = $"slownik_{nazwa}";
        var dto = await GetFromCacheDto<RodzajeWyborowDto>(key, ct);
        if (dto != null)
        {
            // return tracked if present
            var local = db.RodzajeWyborow.Local.FirstOrDefault(r => r.Id == dto.Id);
            if (local != null) return local;

            var tracked = await db.RodzajeWyborow.FindAsync(new object[] { dto.Id }, ct);
            if (tracked != null) return tracked;

            // return plain domain object (not tracked)
            return new RodzajeWyborow { Id = dto.Id, Nazwa = dto.Nazwa, Poziom = (PoziomWyborow)dto.Poziom };
        }

        var existingLocal = db.RodzajeWyborow.Local.FirstOrDefault(s => s.Nazwa == nazwa);
        if (existingLocal != null) return existingLocal;

        var val = await db.RodzajeWyborow.FirstOrDefaultAsync(s => s.Nazwa == nazwa, ct);
        if (val == null)
        {
            val = new RodzajeWyborow { Id = Guid.NewGuid(), Nazwa = nazwa, Poziom = poziom };
            db.RodzajeWyborow.Add(val);
        }

        await SetToCacheDto(key, RodzajeWyborowDto.FromEntity(val), ct);
        return val;
    }

    public async Task<Wybory> GetOrCreateWyboryAsync(Guid rodzajId, DateOnly? dataOgloszenia, DateOnly dataWyborow, OrdynacjaWyborcza ordynacja = OrdynacjaWyborcza.Proporcjonalna, CancellationToken ct = default)
    {
        var key = $"wybory_{rodzajId}_{dataWyborow}";
        var dto = await GetFromCacheDto<WyboryDto>(key, ct);
        if (dto != null)
        {
            var localById = db.Wybory.Local.FirstOrDefault(w => w.Id == dto.Id);
            if (localById != null) return localById;

            var tracked = await db.Wybory.FindAsync(new object[] { dto.Id }, ct);
            if (tracked != null) return tracked;

            return new Wybory
            {
                Id = dto.Id,
                RodzajWyborowId = dto.RodzajWyborowId,
                DataOgloszenia = dto.DataOgloszenia,
                DataWyborow = dto.DataWyborow,
                Ordynacja = (OrdynacjaWyborcza)dto.Ordynacja,
                Tura = dto.Tura.HasValue ? (TuraWyborow?)dto.Tura.Value : null,
                CzyPrzedterminowe = dto.CzyPrzedterminowe
            };
        }

        var local = db.Wybory.Local.FirstOrDefault(w => w.RodzajWyborowId == rodzajId && w.DataWyborow == dataWyborow);
        if (local != null) return local;

        var val = await db.Wybory.FirstOrDefaultAsync(w => w.RodzajWyborowId == rodzajId && w.DataWyborow == dataWyborow, ct);
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

        await SetToCacheDto(key, WyboryDto.FromEntity(val), ct);
        return val;
    }

    public async Task<OkregWyborczy> GetOrCreateOkregAsync(int numer, Guid rodzajWyborowId, CancellationToken ct = default)
    {
        var key = $"okreg_{rodzajWyborowId}_{numer}";
        var dto = await GetFromCacheDto<OkregWyborczyDto>(key, ct);
        if (dto != null)
        {
            var localById = db.OkregWyborczy.Local.FirstOrDefault(o => o.Id == dto.Id);
            if (localById != null) return localById;

            var tracked = await db.OkregWyborczy.FindAsync(new object[] { dto.Id }, ct);
            if (tracked != null) return tracked;

            return new OkregWyborczy
            {
                Id = dto.Id,
                NumerOkregu = dto.NumerOkregu,
                RodzajWyborowId = dto.RodzajWyborowId
            };
        }

        var local = db.OkregWyborczy.Local.FirstOrDefault(o => o.NumerOkregu == numer && o.RodzajWyborowId == rodzajWyborowId);
        if (local != null) return local;

        var val = await db.OkregWyborczy.FirstOrDefaultAsync(o => o.NumerOkregu == numer && o.RodzajWyborowId == rodzajWyborowId, ct);
        if (val == null)
        {
            val = new OkregWyborczy { Id = Guid.NewGuid(), NumerOkregu = numer, RodzajWyborowId = rodzajWyborowId };
            db.OkregWyborczy.Add(val);
        }

        await SetToCacheDto(key, OkregWyborczyDto.FromEntity(val), ct);
        return val;
    }

    public async Task GetOrCreateSzczegolyOkregu(SzczegolyOkreguDto szczegolyOkregu, CancellationToken ct = default)
    {
        var szczegoly = await db.SzczegolyOkregow.FindAsync(
            new object[] { szczegolyOkregu.OkregId, szczegolyOkregu.RokWyborow },
            ct);

        if (szczegoly == null)
        {
            szczegoly = new SzczegolyOkregu { OkregId = szczegolyOkregu.OkregId, RokWyborow = szczegolyOkregu.RokWyborow, Mieszkancy = szczegolyOkregu.Mieszkancy, Uprawnieni = szczegolyOkregu.Uprawnieni };
            db.SzczegolyOkregow.Add(szczegoly);
        }
        else
        {
            szczegoly.Mieszkancy = szczegolyOkregu.Mieszkancy;
            szczegoly.Uprawnieni = szczegolyOkregu.Uprawnieni;
            szczegoly.LiczbaKandydatow = szczegolyOkregu.LiczbaKandydatow;
            szczegoly.LiczbaList = szczegolyOkregu.LiczbaList;
            szczegoly.LiczbaMandatow = szczegolyOkregu.LiczbaMandatow;
            szczegoly.RokWyborow = szczegolyOkregu.RokWyborow;
        }
    }

    public async Task<KomitetWyborczy> GetOrCreateKomitetAsync(string nazwa, CancellationToken ct = default)
    {
        var key = $"komitet_{nazwa}";
        var dto = await GetFromCacheDto<KomitetWyborczyDto>(key, ct);
        if (dto != null)
        {
            var localById = db.KomitetyWyborcze.Local.FirstOrDefault(k => k.Id == dto.Id);
            if (localById != null) return localById;

            var tracked = await db.KomitetyWyborcze.FindAsync(new object[] { dto.Id }, ct);
            if (tracked != null) return tracked;

            return new KomitetWyborczy { Id = dto.Id, Nazwa = dto.Nazwa, Skrot = dto.Skrot, RodzajKomitetuId = dto.RodzajKomitetuId };
        }

        var local = db.KomitetyWyborcze.Local.FirstOrDefault(k => k.Nazwa == nazwa);
        if (local != null) return local;

        var val = await db.KomitetyWyborcze.FirstOrDefaultAsync(k => k.Nazwa == nazwa, ct);
        if (val == null)
        {
            val = new KomitetWyborczy { Id = Guid.NewGuid(), Nazwa = nazwa };
            db.KomitetyWyborcze.Add(val);
        }

        await SetToCacheDto(key, KomitetWyborczyDto.FromEntity(val), ct);
        return val;
    }

    public async Task<ListaWyborcza> GetOrCreateListaAsync(Guid okregId, Guid wyboryId, Guid komitetId, int numer, CancellationToken ct = default)
    {
        var key = $"lista_{wyboryId}_{okregId}_{numer}";
        var dto = await GetFromCacheDto<ListaWyborczaDto>(key, ct);
        if (dto != null)
        {
            var localById = db.ListaWyborcza.Local.FirstOrDefault(l => l.Id == dto.Id);
            if (localById != null) return localById;

            var tracked = await db.ListaWyborcza.FindAsync(new object[] { dto.Id }, ct);
            if (tracked != null) return tracked;

            return new ListaWyborcza
            {
                Id = dto.Id,
                OkregId = dto.OkregId,
                WyboryId = dto.WyboryId,
                KomitetWyborczyId = dto.KomitetWyborczyId,
                NumerListy = dto.NumerListy
            };
        }

        var local = db.ListaWyborcza.Local.FirstOrDefault(l => l.OkregId == okregId && l.WyboryId == wyboryId && l.NumerListy == numer);
        if (local != null) return local;

        var val = await db.ListaWyborcza.FirstOrDefaultAsync(l =>
            l.OkregId == okregId &&
            l.WyboryId == wyboryId &&
            l.NumerListy == numer, ct);

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

        await SetToCacheDto(key, ListaWyborczaDto.FromEntity(val), ct);
        return val;
    }

    public async Task<Klub> GetOrCreatePartiaAsync(string nazwa,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(nazwa) || nazwa.Equals("bezpartyjny", StringComparison.OrdinalIgnoreCase)) return null!;

        var key = $"partia_{nazwa}";
        var dto = await GetFromCacheDto<KlubDto>(key, ct);
        if (dto != null)
        {
            var localById = db.Kluby.Local.FirstOrDefault(k => k.Id == dto.Id);
            if (localById != null) return localById;

            var tracked = await db.Kluby.FindAsync(new object[] { dto.Id }, ct);
            if (tracked != null) return tracked;

            return new Klub { Id = dto.Id, Nazwa = dto.Nazwa, Skrot = dto.Skrot, DataZalozenia = dto.DataZalozenia, DataZakonczeniaDzialalnosci = dto.DataZakonczeniaDzialalnosci };
        }

        var local = db.Kluby.Local.FirstOrDefault(k => k.Nazwa == nazwa);
        if (local != null) return local;

        var val = await db.Kluby.FirstOrDefaultAsync(k => k.Nazwa == nazwa, ct);
        if (val == null)
        {
            val = new Klub { Id = Guid.NewGuid(), Nazwa = nazwa };
            db.Kluby.Add(val);
        }

        await SetToCacheDto(key, KlubDto.FromEntity(val), ct);
        return val;
    }

    public async Task<Polityk> GetOrCreatePolitykAsync(string imie, string nazwisko, CancellationToken ct = default)
    {
        var key = $"polityk_{nazwisko}_{imie}";
        var dto = await GetFromCacheDto<PolitykDto>(key, ct);
        if (dto != null)
        {
            var localById = db.Politycy.Local.FirstOrDefault(p => p.Id == dto.Id);
            if (localById != null) return localById;

            var tracked = await db.Politycy.FindAsync(new object[] { dto.Id }, ct);
            if (tracked != null) return tracked;

            return new Polityk
            {
                Id = dto.Id,
                Imie = dto.Imie,
                Nazwisko = dto.Nazwisko,
                DataUrodzenia = dto.DataUrodzenia,
                MiejsceUrodzenia = dto.MiejsceUrodzenia,
                Email = dto.Email,
                InformacjeDodatkowe = dto.InformacjeDodatkowe
            };
        }

        var local = db.Politycy.Local.FirstOrDefault(p => p.Imie == imie && p.Nazwisko == nazwisko);
        if (local != null) return local;

        var val = await db.Politycy.FirstOrDefaultAsync(p => p.Nazwisko == nazwisko && p.Imie == imie, ct);
        if (val == null)
        {
            val = new Polityk { Id = Guid.NewGuid(), Imie = imie, Nazwisko = nazwisko };
            db.Politycy.Add(val);
        }

        await SetToCacheDto(key, PolitykDto.FromEntity(val), ct);
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
        var serialized = JsonSerializer.Serialize(value,  options: _serializerOptions);
        await cache.SetStringAsync(key, serialized, _cacheOptions, ct);
    }
}
