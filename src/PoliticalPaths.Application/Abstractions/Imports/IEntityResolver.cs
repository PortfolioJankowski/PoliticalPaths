using PoliticalPaths.Domain.Enums;
using PoliticalPaths.Domain.Formacje;
using PoliticalPaths.Domain.Politycy;
using PoliticalPaths.Domain.Wybory;

namespace PoliticalPaths.Application.Abstractions.Imports;

public interface IEntityResolver
{
    Task<RodzajeWyborow> GetOrCreateSlownikWyborowAsync(string nazwa, PoziomWyborow poziom = PoziomWyborow.Krajowy, CancellationToken ct = default);
    
    Task<Wybory> GetOrCreateWyboryAsync(Guid rodzajId, DateOnly? dataOgloszenia, DateOnly dataWyborow, OrdynacjaWyborcza ordynacja = OrdynacjaWyborcza.Proporcjonalna, CancellationToken ct = default);
    
    Task<OkregWyborczy> GetOrCreateOkregAsync(int numer, Guid wyboryId, CancellationToken ct = default);

    Task UpdateOkregDetailsAsync(Guid okregId, int liczbaMandatow, int? liczbaList = null, int? liczbaKandydatow = null, CancellationToken ct = default);

    Task GetOrCreateLudnoscOkregowAsync(Guid okregId, int rok, int mieszkancy, int uprawnieni, CancellationToken ct = default);
    
    Task<KomitetWyborczy> GetOrCreateKomitetAsync(string nazwa, CancellationToken ct = default);
    
    Task<ListaWyborcza> GetOrCreateListaAsync(Guid okregId, Guid wyboryId, Guid komitetId, int numer, CancellationToken ct = default);
    
    Task<Klub> GetOrCreatePartiaAsync(string nazwa, CancellationToken ct = default);
    
    Task<Polityk> GetOrCreatePolitykAsync(string imie, string nazwisko, CancellationToken ct = default);
    
    /// <summary>
    /// Czyści cache (np. po zakończeniu batacha lub pliku).
    /// </summary>
    void ClearCache();
}
