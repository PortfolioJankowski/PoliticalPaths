using PoliticalPaths.Domain.Wybory;
using PoliticalPaths.Shared.Dtos.Domain;

namespace PoliticalPaths.Application.Mappings;

public static class ListaWyborczaExtensions
{
    public static ListaWyborczaDto FromEntity(this ListaWyborcza e)
    {
        return new ListaWyborczaDto
        {
            Id = e.Id,
            KomitetWyborczyId = e.KomitetWyborczyId,
            WyboryId = e.WyboryId,
            NumerListy = e.NumerListy,
            OkregId = e.OkregId
        };
    }
}