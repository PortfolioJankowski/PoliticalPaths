using PoliticalPaths.Domain.Wybory;

namespace PoliticalPaths.Application.Dtos;

public sealed record ListaWyborczaDto
{
    public Guid Id { get; init; }
    public Guid KomitetWyborczyId { get; init; }
    public Guid WyboryId { get; init; }
    public int NumerListy { get; init; }
    public Guid OkregId { get; init; }

    public static ListaWyborczaDto FromEntity(ListaWyborcza e)
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
