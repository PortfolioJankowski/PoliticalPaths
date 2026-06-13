using PoliticalPaths.Domain.Enums;
using PoliticalPaths.Domain.Formacje;

namespace PoliticalPaths.Domain.Wybory;

public sealed class Wybory
{
    public Guid Id { get; set; }
    public Guid RodzajWyborowId { get; set; }
    public RodzajeWyborow Rodzaj { get; set; }
    public DateOnly? DataOgloszenia { get; set; }
    public DateOnly DataWyborow { get; set; }
    public OrdynacjaWyborcza Ordynacja { get; set; }

    public TuraWyborow? Tura { get; set; }
    public bool CzyPrzedterminowe { get; set; }

    public ICollection<KlubCzlonkostwo> Czlonkostwa { get; set; } = new List<KlubCzlonkostwo>();
}
