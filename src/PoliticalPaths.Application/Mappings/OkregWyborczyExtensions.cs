using PoliticalPaths.Domain.Wybory;
using PoliticalPaths.Shared.Dtos.Domain;

namespace PoliticalPaths.Application.Mappings;

public static class OkregWyborczyExtensions
{
    public static OkregWyborczyDto FromEntity(this OkregWyborczy e)
    {
        return new OkregWyborczyDto
        {
            Id = e.Id,
            NumerOkregu = e.NumerOkregu,
            RodzajWyborowId = e.RodzajWyborowId,
        };
    }
}