namespace PoliticalPaths.Domain.Wybory;

public sealed class ListaWyborcza
{
    public Guid Id { get; set; }
    public Guid OkregId { get; set; }
    public int NumerListy { get; set; }
    public Guid MapaWyborowId { get; set; }
    public Guid KomitetWyborczyId { get; set; }
}
