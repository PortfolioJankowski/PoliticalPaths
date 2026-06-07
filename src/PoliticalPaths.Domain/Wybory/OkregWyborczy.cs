namespace PoliticalPaths.Domain.Wybory;

public sealed class OkregWyborczy
{
    public Guid Id { get; set; }
    public int NumerOkregu { get; set; }
    public Guid RodzajWyborowId { get; set; }
    public int LiczbaMandatow { get; set; }
    public int LiczbaList { get; set; }
    public int LiczbaKandydatow { get; set; }
}
