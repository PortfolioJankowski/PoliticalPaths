using PoliticalPaths.Domain.Formacje;
using PoliticalPaths.Domain.StartyWyborcze;

namespace PoliticalPaths.Domain.Politycy;

public sealed class Polityk
{
    public Guid Id { get; set; }
    public string Imie { get; set; } = default!;
    public string Nazwisko { get; set; } = default!;
    public DateOnly? DataUrodzenia { get; set; }
    public string? MiejsceUrodzenia { get; set; }
    public string? Email { get; set; }

    public string? InformacjeDodatkowe { get; set; }
    public ICollection<StartWyborczy> StartyWyborcze { get; set; } = new List<StartWyborczy>();
    public ICollection<KlubCzlonkostwo> Czlonkostwa { get; set; } = new List<KlubCzlonkostwo>();

}
