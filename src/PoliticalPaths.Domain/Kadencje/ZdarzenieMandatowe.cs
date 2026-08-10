using PoliticalPaths.Domain.Politycy;
using PoliticalPaths.Shared.Enums;

namespace PoliticalPaths.Domain.Kadencje;

public sealed class ZdarzenieMandatowe
{
    public long Id { get; set; }
    public Guid MandatId { get; set; }
    public Mandat Mandat {get; set;} = default!;
    public Guid PolitykId { get; set; }
    public Polityk Polityk { get; set; }
    public TypZdarzeniaMandatowego Typ { get; set; }
    public DateOnly DataZdarzenia { get; set; }
    public string? Opis { get; set; }
    public string? DokumentReferencyjny { get; set; }
}
