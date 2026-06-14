using PoliticalPaths.Domain.Wybory;

namespace PoliticalPaths.Application.Dtos;

public sealed record RodzajeWyborowDto
{
    public Guid Id { get; init; }
    public string Nazwa { get; init; } = default!;
    public int Poziom { get; init; }

    // No navigation collections

    public static RodzajeWyborowDto FromEntity(RodzajeWyborow e)
    {
        return new RodzajeWyborowDto
        {
            Id = e.Id,
            Nazwa = e.Nazwa,
            Poziom = (int)e.Poziom
        };
    }
}
