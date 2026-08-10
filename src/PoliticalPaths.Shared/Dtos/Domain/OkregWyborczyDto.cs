namespace PoliticalPaths.Shared.Dtos.Domain;

public sealed record OkregWyborczyDto
{
    public Guid Id { get; init; }
    public int NumerOkregu { get; init; }
    public Guid RodzajWyborowId { get; init; }
}
