using PoliticalPaths.Domain.Wybory;
using PoliticalPaths.Shared.Dtos.Domain;

namespace PoliticalPaths.Application.Mappings;

public static class RodzajeWyborowExtensions
{
    public static RodzajeWyborowDto FromEntity(this RodzajeWyborow e)
    {
        return new RodzajeWyborowDto
        {
            Id = e.Id,
            Nazwa = e.Nazwa,
            Poziom = (int)e.Poziom
        };
    }
}