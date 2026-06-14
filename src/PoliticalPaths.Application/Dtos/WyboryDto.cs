using PoliticalPaths.Domain.Wybory;

namespace PoliticalPaths.Application.Dtos;

public sealed record WyboryDto
{
    public Guid Id { get; init; }
    public Guid RodzajWyborowId { get; init; }
    public RodzajeWyborowDto? Rodzaj { get; init; }
    public DateOnly? DataOgloszenia { get; init; }
    public DateOnly DataWyborow { get; init; }
    public int Ordynacja { get; init; }
    public int? Tura { get; init; }
    public bool CzyPrzedterminowe { get; init; }

    public static WyboryDto FromEntity(Wybory e)
    {
        return new WyboryDto
        {
            Id = e.Id,
            RodzajWyborowId = e.RodzajWyborowId,
            Rodzaj = e.Rodzaj is not null ? new RodzajeWyborowDto { Id = e.Rodzaj.Id, Nazwa = e.Rodzaj.Nazwa, Poziom = (int)e.Rodzaj.Poziom } : null,
            DataOgloszenia = e.DataOgloszenia,
            DataWyborow = e.DataWyborow,
            Ordynacja = (int)e.Ordynacja,
            Tura = e.Tura is not null ? (int?)e.Tura.Value : null,
            CzyPrzedterminowe = e.CzyPrzedterminowe
        };
    }
}
