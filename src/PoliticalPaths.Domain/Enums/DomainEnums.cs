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
    Aktywny = 0,
    Wygasniety = 1,
    Nieobjety = 2
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
