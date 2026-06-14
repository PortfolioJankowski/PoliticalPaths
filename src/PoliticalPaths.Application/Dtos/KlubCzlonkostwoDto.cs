using PoliticalPaths.Domain.Formacje;

namespace PoliticalPaths.Application.Dtos;

public sealed record KlubCzlonkostwoDto
{
    public Guid Id { get; init; }
    public Guid KlubId { get; init; }
    public Guid PolitykId { get; init; }
    public Guid WyboryId { get; init; }
    public bool IsActive { get; init; }

    public static KlubCzlonkostwoDto FromEntity(KlubCzlonkostwo e)
    {
        return new KlubCzlonkostwoDto
        {
            Id = e.Id,
            KlubId = e.KlubId,
            PolitykId = e.PolitykId,
            WyboryId = e.WyboryId,
            IsActive = e.IsActive
        };
    }
}
