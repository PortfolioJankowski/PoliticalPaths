using PoliticalPaths.Domain.Wybory;
using PoliticalPaths.Shared.Dtos.Domain;

namespace PoliticalPaths.Application.Mappings;

public static class WyboryExtensions
{
    public static WyboryDto FromEntity(this Wybory e)
    {
        return new WyboryDto
        {
            Id = e.Id,
            RodzajWyborowId = e.RodzajWyborowId,
            Rodzaj = e.Rodzaj is not null ? new RodzajeWyborowDto { Id = e.Rodzaj.Id, Nazwa = e.Rodzaj.Nazwa, Poziom = (int)e.Rodzaj.Poziom } : null,
            DataOgloszenia = e.DataOgloszenia,
            DataWyborow = e.DataWyborow,
            Ordynacja = e.Ordynacja,
            Tura = e.Tura.Value,
            CzyPrzedterminowe = e.CzyPrzedterminowe,
            Kadencja = e.Kadencja
        };
    }
}