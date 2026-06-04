using PoliticalPaths.Domain.Enums;

namespace PoliticalPaths.Domain.Geografia;

public sealed class Teryt
{
    public string KodTeryt { get; set; } = default!;
    public string Nazwa { get; set; } = default!;
    public PoziomJednostki Poziom { get; set; }
    public string? KodNadrzedny { get; set; }
}
