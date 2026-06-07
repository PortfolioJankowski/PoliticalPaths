namespace PoliticalPaths.Domain.StartyWyborcze;

public sealed class StartyWyborcze
{
    public Guid Id { get; set; }
    public Guid PolitykId { get; set; }
    public int? NumerNaLiscie { get; set; }
    public Guid? ListaId { get; set; }
    public string? Zawod { get; set; }
    public string? Wyksztalcenie { get; set; }
    public string? MiejsceZamieszkania { get; set; }
    public Guid? PartiaId { get; set; }
    public Guid KomitetId { get; set; }
}
