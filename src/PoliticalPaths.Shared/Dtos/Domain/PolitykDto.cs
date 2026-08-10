namespace PoliticalPaths.Shared.Dtos.Domain;

public sealed record PolitykDto
{
    public Guid Id { get; init; }
    public string Imie { get; init; } = default!;
    public string Nazwisko { get; init; } = default!;
    public DateOnly? DataUrodzenia { get; init; }
    public string? MiejsceUrodzenia { get; init; }
    public string? Email { get; init; }
    public string? InformacjeDodatkowe { get; init; }
}
