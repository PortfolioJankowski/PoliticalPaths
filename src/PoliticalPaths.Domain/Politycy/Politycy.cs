namespace PoliticalPaths.Domain.Politycy;

public sealed class Politycy
{
    public Guid Id { get; set; }
    public string Imie { get; set; } = default!;
    public string Nazwisko { get; set; } = default!;
    public DateOnly? DataUrodzenia { get; set; }
    public string? MiejsceUrodzeniaKodTeryt { get; set; }
    public string? Email { get; set; }
    public string? InformacjeDodatkowe { get; set; }
}
