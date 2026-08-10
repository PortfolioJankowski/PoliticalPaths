using PoliticalPaths.Domain.Wybory;
using PoliticalPaths.Shared.Dtos.Domain;

namespace PoliticalPaths.Application.Mappings;

public static class KomitetWyborczyExtensions
{
    public static KomitetWyborczyDto FromEntity(this KomitetWyborczy komitetWyborczy)
    {
        return new KomitetWyborczyDto
        {
            Id = komitetWyborczy.Id,
            Nazwa = komitetWyborczy.Nazwa,
            Skrot = komitetWyborczy.Skrot,
        };
    } 
}