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
    public Wybory Wybory { get; set; }
    // jeżeli będą wybory uzupełniające to dla danego roku wyborczego może być więcej niż 1 encja wybory
    // dlatego każde szczegóły okręgu przynależą jednym wyborom
    public Guid WyboryId { get; set; } 
}
