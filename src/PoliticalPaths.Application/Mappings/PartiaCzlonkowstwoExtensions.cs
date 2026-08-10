using PoliticalPaths.Domain.Formacje;
using PoliticalPaths.Shared.Dtos.Domain;

namespace PoliticalPaths.Application.Mappings;

public static class PartiaCzlonkowstwoExtensions
{
    public static PartiaCzlonkostwoDto FromEntity(this PartiaCzlonkostwo e)
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