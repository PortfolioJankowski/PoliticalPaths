using PoliticalPaths.Domain.Enums;

namespace PoliticalPaths.Domain.Kadencje;

public sealed class ZdarzenieMandatowe
{
    public long Id { get; set; }
    public Guid MandatId { get; set; }
    public Guid PolitykId { get; set; }
    public TypZdarzeniaMandatowego Typ { get; set; }
    public DateOnly DataZdarzenia { get; set; }
    public string? Opis { get; set; }
    public string? DokumentReferencyjny { get; set; }
}

public enum TypZdarzeniaMandatowego
{
    Objecie = 1,          // Ślubowanie
    Wygasniecie = 2,      // Śmierć, utrata praw wyborczych
    Zrzeczenie = 3,       // Rezygnacja
    ObjecieInnejFunkcji = 4, // Np. wybór do PE, na wójta itd.
    KoniecKadencji = 5    // Naturalny koniec
}
