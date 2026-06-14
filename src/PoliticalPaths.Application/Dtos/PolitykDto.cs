using PoliticalPaths.Domain.Politycy;

namespace PoliticalPaths.Application.Dtos;

public sealed record PolitykDto
{
    public Guid Id { get; init; }
    public string Imie { get; init; } = default!;
    public string Nazwisko { get; init; } = default!;
    public DateOnly? DataUrodzenia { get; init; }
    public string? MiejsceUrodzenia { get; init; }
    public string? Email { get; init; }
    public string? InformacjeDodatkowe { get; init; }

    public static PolitykDto FromEntity(Polityk e)
    {
        return new PolitykDto
        {
            Id = e.Id,
            Imie = e.Imie,
            Nazwisko = e.Nazwisko,
            DataUrodzenia = e.DataUrodzenia,
            MiejsceUrodzenia = e.MiejsceUrodzenia,
            Email = e.Email,
            InformacjeDodatkowe = e.InformacjeDodatkowe
        };
    }
}
