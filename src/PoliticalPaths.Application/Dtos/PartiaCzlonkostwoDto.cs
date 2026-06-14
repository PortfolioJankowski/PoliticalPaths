using PoliticalPaths.Domain.Formacje;

namespace PoliticalPaths.Application.Dtos;

public sealed record PartiaCzlonkostwoDto
{
    public Guid Id { get; init; }
    public Guid PartiaId { get; init; }
    public Guid PolitykId { get; init; }
    public Guid WyboryId { get; init; }
    public bool IsActive { get; init; }

    public static PartiaCzlonkostwoDto FromEntity(PartiaCzlonkostwo e)
    {
        return new PartiaCzlonkostwoDto
        {
            Id = e.Id,
            PartiaId = e.PartiaId,
            PolitykId = e.PolitykId,
            WyboryId = e.WyboryId,
            IsActive = e.IsActive
        };
    }
}
