namespace PoliticalPaths.Domain.Enums;

public enum RodzajIzby
{
    Sejm = 0,
    Senat = 1,
    Sejmik = 2
}

public enum ZasiegWyborow
{
    Krajowy = 0,
    Wojewodzki = 1,
    Europejski = 2
}

public enum OrdynacjaWyborcza
{
    Proporcjonalna = 0,
    Wiekszosciowa = 1
}

public enum RodzajKomitetu
{
    Partyjny = 0,
    Koalicyjny = 1,
    WyborczyWyborcow = 2,
    Inny = 3
}

public enum PoziomJednostki
{
    Wojewodztwo = 0,
    Powiat = 1,
    Gmina = 2,
    Miasto = 3,
    Dzielnica = 4,
    Inny = 5
}

public enum StatusMandatu
{
    Oczekujacy = 0, // Wybrany, ale jeszcze nie zaprzysiężony
    Aktywny = 1,    // Sprawuje mandat
    Wygasniety = 2, // Mandat wygasł przedwcześnie (śmierć, rezygnacja)
    Zakonczony = 3, // Koniec kadencji
    Nieobjety = 4   // Wybrany, ale zrezygnował przed ślubowaniem
}

public enum TypObjeciaMandatu
{
    WyborBezposredni = 0,
    Sukcesja = 1 // Wstąpienie na wolne miejsce
}

public enum TypZdarzeniaMandatowego
{
    Wybor = 1,            // Zdobycie mandatu w głosowaniu
    Objecie = 2,          // Ślubowanie / Zaprzysiężenie
    Wstąpienie = 3,       // Objęcie mandatu w trakcie kadencji (sukcesja)
    Wygasniecie = 4,      // Śmierć, utrata praw wyborczych
    Zrzeczenie = 5,       // Rezygnacja
    ObjecieInnejFunkcji = 6, // Np. wybór do PE, na wójta itd.
    KoniecKadencji = 7,    // Naturalny koniec,
    Zgon = 8
}

public enum PowodWygasnieciaMandatu
{
    Smierc = 0,
    UtrataPrawaWybieralnosci = 1,
    Rezygnacja = 2,
    WyborNaPrezydenta = 3,
    WyborDoParlamentuEuropejskiego = 4,
    Inny = 5
}

public enum TuraWyborow
{
    Pierwsza = 1,
    Druga = 2
}

public enum PoziomWyborow
{
    GminaMiasto = 0,
    Powiat = 1,
    Wojewodztwo = 2,
    Krajowy = 3,
    Europejski = 4
}
