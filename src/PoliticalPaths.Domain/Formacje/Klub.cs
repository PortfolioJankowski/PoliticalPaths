namespace PoliticalPaths.Domain.Formacje;

/// <summary>
/// Osoba startuj¹ca mo¿e mieæ przynale¿noœæ do klubu
/// </summary>
public sealed class Klub
{
    public Guid Id { get; set; }
    public string Nazwa { get; set; } = default!;
    public string? Skrot { get; set; }
    public DateOnly? DataZalozenia { get; set; }
    public DateOnly? DataZakonczeniaDzialalnosci { get; set; }
    public ICollection<KlubCzlonkostwo> Czlonkostwa { get; set; } = new List<KlubCzlonkostwo>();

}
