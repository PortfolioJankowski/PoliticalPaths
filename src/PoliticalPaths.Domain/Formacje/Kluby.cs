namespace PoliticalPaths.Domain.Formacje;

public sealed class Kluby
{
    public Guid Id { get; set; }
    public string Nazwa { get; set; } = default!;
    public string? Skrot { get; set; }
}
