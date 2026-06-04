namespace PoliticalPaths.Domain.Wybory;

public sealed class MapaOkregow
{
    public string KodTeryt { get; set; } = default!;
    public Guid OkregWyborczyId { get; set; }
}
