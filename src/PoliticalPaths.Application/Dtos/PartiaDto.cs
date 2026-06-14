using PoliticalPaths.Domain.Formacje;

namespace PoliticalPaths.Application.Dtos;

public sealed record PartiaDto
{
    public Guid Id { get; init; }
    public string Nazwa { get; init; } = default!;
    public string? Skrot { get; init; }
    public DateOnly? DataZalozenia { get; init; }
    public DateOnly? DataZakonczeniaDzialalnosci { get; init; }

    public static PartiaDto FromEntity(Partia e)
    {
        return new PartiaDto
        {
            Id = e.Id,
            Nazwa = e.Nazwa,
            Skrot = e.Skrot,
            DataZalozenia = e.DataZalozenia,
            DataZakonczeniaDzialalnosci = e.DataZakonczeniaDzialalnosci
        };
    }
}
