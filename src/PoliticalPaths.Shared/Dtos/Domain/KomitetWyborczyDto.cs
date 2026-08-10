namespace PoliticalPaths.Shared.Dtos.Domain;

public sealed record KomitetWyborczyDto
{
    public Guid Id { get; init; }
    public string Nazwa { get; init; } = default!;
    public string? Skrot { get; init; }
}
