namespace PoliticalPaths.Domain.StartyWyborcze;

public sealed class StartyWyborcze
{
    public Guid Id { get; set; }
    public Guid PolitykId { get; set; }
    public int NumerNaLiscie { get; set; }
    public Guid ListaId { get; set; }
}
