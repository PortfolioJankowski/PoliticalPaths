namespace PoliticalPaths.Shared.Dtos.Domain;

public sealed record PartiaDto
{
    public Guid Id { get; init; }
    public string Nazwa { get; init; } = default!;
    public string? Skrot { get; init; }
    public DateOnly? DataZalozenia { get; init; }
    public DateOnly? DataZakonczeniaDzialalnosci { get; init; }
}
