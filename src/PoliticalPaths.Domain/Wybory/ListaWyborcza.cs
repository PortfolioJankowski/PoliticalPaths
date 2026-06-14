namespace PoliticalPaths.Domain.Wybory;

public sealed class ListaWyborcza
{
    public Guid Id { get; set; }
    public Guid OkregId { get; set; }
    public int NumerListy { get; set; }
    public Guid WyboryId { get; set; }
    public Guid KomitetWyborczyId { get; set; }
}
