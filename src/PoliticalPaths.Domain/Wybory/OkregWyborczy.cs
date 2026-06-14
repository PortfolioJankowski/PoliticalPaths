namespace PoliticalPaths.Domain.Wybory;

public sealed class OkregWyborczy
{
    public Guid Id { get; set; }
    public int NumerOkregu { get; set; }
    public Guid RodzajWyborowId { get; set; }
    public ICollection<SzczegolyOkregu> Ludnosc { get; set; } = new List<SzczegolyOkregu>();
}
