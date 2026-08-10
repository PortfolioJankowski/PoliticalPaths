namespace PoliticalPaths.Shared.Dtos.Domain;

public sealed record RodzajeWyborowDto
{
    public Guid Id { get; init; }
    public string Nazwa { get; init; } = default!;
    public int Poziom { get; init; }
}
