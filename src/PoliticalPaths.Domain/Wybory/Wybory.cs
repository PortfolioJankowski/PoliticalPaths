using PoliticalPaths.Domain.Formacje;
using PoliticalPaths.Shared.Enums;

namespace PoliticalPaths.Domain.Wybory;

public sealed class Wybory
{
    public Guid Id { get; set; }
    public Guid RodzajWyborowId { get; set; }
    public RodzajeWyborow Rodzaj { get; set; }
    public DateOnly? DataOgloszenia { get; set; }
    public DateOnly DataWyborow { get; set; }
    public OrdynacjaWyborcza Ordynacja { get; set; }
    public string? Kadencja { get; set; }
    public TuraWyborow? Tura { get; set; }
    public bool CzyPrzedterminowe { get; set; }
    
    public ICollection<PartiaCzlonkostwo> Czlonkostwa { get; set; } = new List<PartiaCzlonkostwo>();
}
