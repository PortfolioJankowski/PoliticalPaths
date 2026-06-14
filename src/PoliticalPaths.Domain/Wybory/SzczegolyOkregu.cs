namespace PoliticalPaths.Domain.Wybory;

public sealed class SzczegolyOkregu
{
    public Guid OkregId { get; set; }
    public OkregWyborczy Okreg { get; set; } = null!;
    public int RokWyborow { get; set; }
    public int Mieszkancy { get; set; }
    public int Uprawnieni { get; set; }
    public int LiczbaMandatow { get; set; }
    public int LiczbaList { get; set; }
    public int LiczbaKandydatow { get; set; }
}
