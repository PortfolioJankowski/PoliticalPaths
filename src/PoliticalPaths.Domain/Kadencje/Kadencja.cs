namespace PoliticalPaths.Domain.Kadencje;

public sealed class Kadencja
{
    public Guid Id { get; set; }
    public string Nazwa { get; set; } = default!;
    public DateOnly DataRozpoczecia { get; set; }
    public DateOnly? DataZakonczenia { get; set; }
}
