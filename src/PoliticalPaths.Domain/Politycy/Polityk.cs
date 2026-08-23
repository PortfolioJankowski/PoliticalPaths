using PoliticalPaths.Domain.Formacje;
using PoliticalPaths.Domain.Kadencje;
using PoliticalPaths.Domain.StartyWyborcze;

namespace PoliticalPaths.Domain.Politycy;

public sealed class Polityk
{
    public Guid Id { get; set; }
    public string Imie { get; set; } = default!;
    public string DrugieImie { get; set; } = "Nieznane";
    public string Nazwisko { get; set; } = default!;
    public DateOnly? DataUrodzenia { get; set; }
    public string? MiejsceUrodzenia { get; set; }
    public string? Email { get; set; }

    public string? InformacjeDodatkowe { get; set; }
    public ICollection<StartWyborczy> StartyWyborcze { get; set; } = new List<StartWyborczy>();
    public ICollection<PartiaCzlonkostwo> Czlonkostwa { get; set; } = new List<PartiaCzlonkostwo>();
    public ICollection<Mandat> Mandaty { get; set; } = new List<Mandat>();
    public ICollection<ZdarzenieMandatowe> ZdarzeniaMandatowe { get; set; } = new List<ZdarzenieMandatowe>();

}
