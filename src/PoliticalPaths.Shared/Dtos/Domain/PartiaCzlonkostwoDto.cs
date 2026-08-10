namespace PoliticalPaths.Shared.Dtos.Domain;

public sealed record PartiaCzlonkostwoDto
{
    public Guid Id { get; init; }
    public Guid PartiaId { get; init; }
    public Guid PolitykId { get; init; }
    public Guid WyboryId { get; init; }
    public bool IsActive { get; init; }
}
