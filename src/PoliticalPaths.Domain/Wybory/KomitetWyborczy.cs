namespace PoliticalPaths.Domain.Wybory;

public sealed class KomitetWyborczy
{
    public Guid Id { get; set; }
    public string Nazwa { get; set; } = default!;
    public string? Skrot { get; set; }
}
