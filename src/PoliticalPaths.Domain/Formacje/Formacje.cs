namespace PoliticalPaths.Domain.Formacje;

public sealed class Formacje
{
    public Guid Id { get; set; }
    public string Nazwa { get; set; } = default!;
    public string? Skrot { get; set; }
    public DateOnly? DataZalozenia { get; set; }
    public DateOnly? DataZakonczeniaDzialalnosci { get; set; }
}
