using PoliticalPaths.Domain.Politycy;

namespace PoliticalPaths.Domain.StartyWyborcze;

public sealed class StartWyborczy
{
    public Guid Id { get; set; }
    public Guid PolitykId { get; set; }
    public Polityk Polityk { get; set; } = default!;
    public int? NumerNaLiscie { get; set; }
    public Guid? ListaId { get; set; }
    public string? Zawod { get; set; }
    public string? Wyksztalcenie { get; set; }
    public string? MiejsceZamieszkania { get; set; }
    public Guid? PartiaId { get; set; }
    public Guid KomitetId { get; set; }
    public Guid WynikiId { get; set; }
    public Guid? PopierajacaPartiaId { get; set; }

}
