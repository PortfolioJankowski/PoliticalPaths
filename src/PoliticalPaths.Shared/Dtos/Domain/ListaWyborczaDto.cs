namespace PoliticalPaths.Shared.Dtos.Domain;

public sealed record ListaWyborczaDto
{
    public Guid Id { get; init; }
    public Guid KomitetWyborczyId { get; init; }
    public Guid WyboryId { get; init; }
    public int NumerListy { get; init; }
    public Guid OkregId { get; init; }
}
