using PoliticalPaths.Domain.Formacje;

namespace PoliticalPaths.Application.Dtos;

public sealed record KlubDto
{
    public Guid Id { get; init; }
    public string Nazwa { get; init; } = default!;
    public string? Skrot { get; init; }
    public DateOnly? DataZalozenia { get; init; }
    public DateOnly? DataZakonczeniaDzialalnosci { get; init; }

    public static KlubDto FromEntity(Klub e)
    {
        return new KlubDto
        {
            Id = e.Id,
            Nazwa = e.Nazwa,
            Skrot = e.Skrot,
            DataZalozenia = e.DataZalozenia,
            DataZakonczeniaDzialalnosci = e.DataZakonczeniaDzialalnosci
        };
    }
}
