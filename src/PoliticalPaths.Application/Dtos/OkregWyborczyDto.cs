using PoliticalPaths.Domain.Wybory;

namespace PoliticalPaths.Application.Dtos;

public sealed record OkregWyborczyDto
{
    public Guid Id { get; init; }
    public int NumerOkregu { get; init; }
    public Guid RodzajWyborowId { get; init; }

    public static OkregWyborczyDto FromEntity(OkregWyborczy e)
    {
        return new OkregWyborczyDto
        {
            Id = e.Id,
            NumerOkregu = e.NumerOkregu,
            RodzajWyborowId = e.RodzajWyborowId,
        };
    }
}
