namespace PoliticalPaths.Domain.Wybory;

public sealed class KomitetyWyborcze
{
    public Guid Id { get; set; }
    public string Nazwa { get; set; } = default!;
    public string? Skrot { get; set; }
    public Guid RodzajKomitetuId { get; set; }
}
