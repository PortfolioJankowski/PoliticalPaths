using ClosedXML.Excel;

namespace PoliticalPaths.Importers.Raw;

internal static class SejmDemo2023SampleBuilder
{
    public static void CreateWorkbook(string path, string sidecarPath)
    {
        using var workbook = new XLWorkbook();

        WriteDistricts(workbook.AddWorksheet("Okregi"));
        WriteLists(workbook.AddWorksheet("Listy"));
        WriteCandidates(workbook.AddWorksheet("Kandydaci"));
        WriteTurnout(workbook.AddWorksheet("Frekwencja"));
        WriteClubs(workbook.AddWorksheet("Kluby"));

        workbook.SaveAs(path);

        File.WriteAllText(sidecarPath, """
            {
              "logicalName": "sejm-demo-2023",
              "formatVersion": "v1",
              "electionYear": "2023"
            }
            """);
    }

    private static void WriteDistricts(IXLWorksheet sheet)
    {
        sheet.Cell(1, 1).Value = "Numer";
        sheet.Cell(1, 2).Value = "Nazwa";
        sheet.Cell(1, 3).Value = "Ludnosc";
        sheet.Cell(1, 4).Value = "Uprawnieni";
        sheet.Cell(1, 5).Value = "Mandaty";
        sheet.Cell(1, 6).Value = "TERYT";

        sheet.Cell(2, 1).Value = "1";
        sheet.Cell(2, 2).Value = "Warszawa";
        sheet.Cell(2, 3).Value = "1500000";
        sheet.Cell(2, 4).Value = "1200000";
        sheet.Cell(2, 5).Value = "20";
        sheet.Cell(2, 6).Value = "1465011";

        sheet.Cell(3, 1).Value = "2";
        sheet.Cell(3, 2).Value = "Krakow";
        sheet.Cell(3, 3).Value = "800000";
        sheet.Cell(3, 4).Value = "650000";
        sheet.Cell(3, 5).Value = "12";
        sheet.Cell(3, 6).Value = "1261011";
    }

    private static void WriteLists(IXLWorksheet sheet)
    {
        sheet.Cell(1, 1).Value = "Okreg";
        sheet.Cell(1, 2).Value = "NumerListy";
        sheet.Cell(1, 3).Value = "NazwaListy";
        sheet.Cell(1, 4).Value = "Komitet";
        sheet.Cell(1, 5).Value = "SkrotKomitetu";
        sheet.Cell(1, 6).Value = "Partia";

        sheet.Cell(2, 1).Value = "1";
        sheet.Cell(2, 2).Value = "1";
        sheet.Cell(2, 3).Value = "Lista 1 - Koalicja Demokratyczna";
        sheet.Cell(2, 4).Value = "Komitet Wyborczy Koalicja Demokratyczna";
        sheet.Cell(2, 5).Value = "KDK";
        sheet.Cell(2, 6).Value = "Partia Alfa";

        sheet.Cell(3, 1).Value = "1";
        sheet.Cell(3, 2).Value = "2";
        sheet.Cell(3, 3).Value = "Lista 2 - Partia Beta";
        sheet.Cell(3, 4).Value = "Komitet Wyborczy Partii Beta";
        sheet.Cell(3, 5).Value = "KPB";
        sheet.Cell(3, 6).Value = "Partia Beta";

        sheet.Cell(4, 1).Value = "2";
        sheet.Cell(4, 2).Value = "1";
        sheet.Cell(4, 3).Value = "Lista 1 - Koalicja Demokratyczna";
        sheet.Cell(4, 4).Value = "Komitet Wyborczy Koalicja Demokratyczna";
        sheet.Cell(4, 5).Value = "KDK";
        sheet.Cell(4, 6).Value = "Partia Alfa";

        sheet.Cell(5, 1).Value = "2";
        sheet.Cell(5, 2).Value = "2";
        sheet.Cell(5, 3).Value = "Lista 2 - Zieloni";
        sheet.Cell(5, 4).Value = "Komitet Wyborczy Zieloni";
        sheet.Cell(5, 5).Value = "KWZ";
        sheet.Cell(5, 6).Value = "Partia Zieloni";
    }

    private static void WriteCandidates(IXLWorksheet sheet)
    {
        sheet.Cell(1, 1).Value = "Okreg";
        sheet.Cell(1, 2).Value = "Lista";
        sheet.Cell(1, 3).Value = "Pozycja";
        sheet.Cell(1, 4).Value = "Nazwisko";
        sheet.Cell(1, 5).Value = "Imie";
        sheet.Cell(1, 6).Value = "Glosy";
        sheet.Cell(1, 7).Value = "Procent";
        sheet.Cell(1, 8).Value = "Wybrany";

        // Okręg 1, lista 1
        sheet.Cell(2, 1).Value = "1";
        sheet.Cell(2, 2).Value = "1";
        sheet.Cell(2, 3).Value = "1";
        sheet.Cell(2, 4).Value = "Kowalski";
        sheet.Cell(2, 5).Value = "Jan";
        sheet.Cell(2, 6).Value = "45200";
        sheet.Cell(2, 7).Value = "12.4";
        sheet.Cell(2, 8).Value = "TAK";

        sheet.Cell(3, 1).Value = "1";
        sheet.Cell(3, 2).Value = "1";
        sheet.Cell(3, 3).Value = "2";
        sheet.Cell(3, 4).Value = "Nowak";
        sheet.Cell(3, 5).Value = "Anna";
        sheet.Cell(3, 6).Value = "38100";
        sheet.Cell(3, 7).Value = "10.5";
        sheet.Cell(3, 8).Value = "TAK";

        sheet.Cell(4, 1).Value = "1";
        sheet.Cell(4, 2).Value = "2";
        sheet.Cell(4, 3).Value = "1";
        sheet.Cell(4, 4).Value = "Lewandowski";
        sheet.Cell(4, 5).Value = "Piotr";
        sheet.Cell(4, 6).Value = "29500";
        sheet.Cell(4, 7).Value = "8.1";
        sheet.Cell(4, 8).Value = "nie";

        // Okręg 2
        sheet.Cell(5, 1).Value = "2";
        sheet.Cell(5, 2).Value = "1";
        sheet.Cell(5, 3).Value = "1";
        sheet.Cell(5, 4).Value = "Wisniewski";
        sheet.Cell(5, 5).Value = "Maria";
        sheet.Cell(5, 6).Value = "22000";
        sheet.Cell(5, 7).Value = "9.8";
        sheet.Cell(5, 8).Value = "TAK";

        sheet.Cell(6, 1).Value = "2";
        sheet.Cell(6, 2).Value = "2";
        sheet.Cell(6, 3).Value = "1";
        sheet.Cell(6, 4).Value = "Zielinski";
        sheet.Cell(6, 5).Value = "Tomasz";
        sheet.Cell(6, 6).Value = "18500";
        sheet.Cell(6, 7).Value = "8.2";
        sheet.Cell(6, 8).Value = "nie";

        // Celowy błąd — brak nazwiska
        sheet.Cell(7, 1).Value = "2";
        sheet.Cell(7, 2).Value = "2";
        sheet.Cell(7, 3).Value = "2";
        sheet.Cell(7, 4).Value = "";
        sheet.Cell(7, 5).Value = "Ewa";
        sheet.Cell(7, 6).Value = "500";
        sheet.Cell(7, 7).Value = "0.2";
        sheet.Cell(7, 8).Value = "nie";
    }

    private static void WriteTurnout(IXLWorksheet sheet)
    {
        sheet.Cell(1, 1).Value = "Okreg";
        sheet.Cell(1, 2).Value = "Wydane";
        sheet.Cell(1, 3).Value = "Wazne";
        sheet.Cell(1, 4).Value = "Niewazne";
        sheet.Cell(1, 5).Value = "Frekwencja";

        sheet.Cell(2, 1).Value = "1";
        sheet.Cell(2, 2).Value = "800000";
        sheet.Cell(2, 3).Value = "750000";
        sheet.Cell(2, 4).Value = "5000";
        sheet.Cell(2, 5).Value = "62.5";

        sheet.Cell(3, 1).Value = "2";
        sheet.Cell(3, 2).Value = "420000";
        sheet.Cell(3, 3).Value = "395000";
        sheet.Cell(3, 4).Value = "3000";
        sheet.Cell(3, 5).Value = "60.8";
    }

    private static void WriteClubs(IXLWorksheet sheet)
    {
        sheet.Cell(1, 1).Value = "Klub";
        sheet.Cell(1, 2).Value = "Nazwisko";
        sheet.Cell(1, 3).Value = "Imie";
        sheet.Cell(1, 4).Value = "Od";

        sheet.Cell(2, 1).Value = "Klub Poselski Demo Alfa";
        sheet.Cell(2, 2).Value = "Kowalski";
        sheet.Cell(2, 3).Value = "Jan";
        sheet.Cell(2, 4).Value = "2023-11-20";

        sheet.Cell(3, 1).Value = "Klub Poselski Demo Alfa";
        sheet.Cell(3, 2).Value = "Nowak";
        sheet.Cell(3, 3).Value = "Anna";
        sheet.Cell(3, 4).Value = "2023-11-20";

        sheet.Cell(4, 1).Value = "Klub Poselski Demo Regionalny";
        sheet.Cell(4, 2).Value = "Wisniewski";
        sheet.Cell(4, 3).Value = "Maria";
        sheet.Cell(4, 4).Value = "2023-11-21";
    }
}
