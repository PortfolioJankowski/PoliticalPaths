namespace PoliticalPaths.Domain.Wybory;

public sealed class LudnoscOkregow
{
    public Guid OkregId { get; set; }
    public OkregWyborczy Okreg { get; set; } = null!;
    public int RokWyborow { get; set; }
    public int Mieszkancy { get; set; }
    public int Uprawnieni { get; set; }
}
