using PoliticalPaths.Application.Dtos;
using PoliticalPaths.Domain.Enums;
using PoliticalPaths.Domain.Formacje;
using PoliticalPaths.Domain.Politycy;
using PoliticalPaths.Domain.StartyWyborcze;
using PoliticalPaths.Domain.Wybory;

namespace PoliticalPaths.Application.Abstractions.Imports;

public interface IEntityResolver
{
    Task<RodzajeWyborow> GetOrCreateSlownikWyborowAsync(string nazwa, PoziomWyborow poziom = PoziomWyborow.Krajowy, CancellationToken ct = default);
    
    Task<Wybory> GetOrCreateWyboryAsync(WyboryDto wyboryDto, CancellationToken ct = default);
    
    Task<OkregWyborczy> GetOrCreateOkregAsync(int numer, Guid rodzajWyborowId, CancellationToken ct = default);

    Task GetOrCreateSzczegolyOkregu(SzczegolyOkreguDto szczegolyOkregu, CancellationToken ct = default);
    
    Task<KomitetWyborczy> GetOrCreateKomitetAsync(string nazwa, CancellationToken ct = default);
    
    Task<ListaWyborcza> GetOrCreateListaAsync(Guid okregId, Guid wyboryId, Guid komitetId, int numer, CancellationToken ct = default);
    
    Task<Partia> GetOrCreatePartiaAsync(string nazwa, CancellationToken ct = default);
    
    Task<Polityk> GetOrCreatePolitykAsync(string imie, string nazwisko, CancellationToken ct = default);

    WynikiWyborow CreateWynikiAsync(int glosy, bool czyMandat);

    /// <summary>
    /// Czyści cache (np. po zakończeniu batacha lub pliku).
    /// </summary>
    void ClearCache();
}
