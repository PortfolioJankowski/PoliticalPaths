using PoliticalPaths.Domain.Wybory;

namespace PoliticalPaths.Application.Dtos;

public sealed record KomitetWyborczyDto
{
    public Guid Id { get; init; }
    public string Nazwa { get; init; } = default!;
    public string? Skrot { get; init; }

    public static KomitetWyborczyDto FromEntity(KomitetWyborczy e)
    {
        return new KomitetWyborczyDto
        {
            Id = e.Id,
            Nazwa = e.Nazwa,
            Skrot = e.Skrot,
        };
    }
}
