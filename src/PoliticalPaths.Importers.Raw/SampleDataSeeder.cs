using ClosedXML.Excel;
using PoliticalPaths.Application.Imports.Inbox;

namespace PoliticalPaths.Importers.Raw;

public sealed class SampleDataSeeder : ISampleDataSeeder
{
    public string? EnsureSampleInPipelineFolder(string pipelineDirectory, string pipelineKey)
    {
        Directory.CreateDirectory(pipelineDirectory);

        if (Directory.EnumerateFiles(pipelineDirectory, "*.xlsx", SearchOption.TopDirectoryOnly).Any())
            return null;

        if (string.Equals(pipelineKey, "sejm-demo-2023", StringComparison.OrdinalIgnoreCase))
        {
            var demoPath = Path.Combine(pipelineDirectory, "sejm-demo-2023.xlsx");
            var demoSidecar = Path.Combine(pipelineDirectory, "sejm-demo-2023.import.json");
            SejmDemo2023SampleBuilder.CreateWorkbook(demoPath, demoSidecar);
            return demoPath;
        }

        var path = Path.Combine(pipelineDirectory, "test-sample.xlsx");
        var sidecarPath = Path.Combine(pipelineDirectory, "test-sample.import.json");

        using (var workbook = new XLWorkbook())
        {
            var sheet = workbook.AddWorksheet("Kandydaci");
            sheet.Cell(1, 1).Value = "Nazwisko";
            sheet.Cell(1, 2).Value = "Imie";
            sheet.Cell(1, 3).Value = "Okręg";
            sheet.Cell(1, 4).Value = "Lista";
            sheet.Cell(1, 5).Value = "Głosy";
            sheet.Cell(2, 1).Value = "Kowalski";
            sheet.Cell(2, 2).Value = "Jan";
            sheet.Cell(2, 3).Value = "19";
            sheet.Cell(2, 4).Value = "1";
            sheet.Cell(2, 5).Value = "12000";
            sheet.Cell(3, 1).Value = "Nowak";
            sheet.Cell(3, 2).Value = "Anna";
            sheet.Cell(3, 3).Value = "19";
            sheet.Cell(3, 4).Value = "2";
            sheet.Cell(3, 5).Value = "9500";
            // Celowo błędne wiersze — demo logowania błędów transformacji (F5 → TransformationErrors + logi)
            sheet.Cell(4, 1).Value = "";
            sheet.Cell(4, 2).Value = "Piotr";
            sheet.Cell(4, 3).Value = "19";
            sheet.Cell(4, 4).Value = "1";
            sheet.Cell(4, 5).Value = "100";
            sheet.Cell(5, 1).Value = "Zielinski";
            sheet.Cell(5, 2).Value = "Tomasz";
            sheet.Cell(5, 3).Value = "19";
            sheet.Cell(5, 4).Value = "3";
            sheet.Cell(5, 5).Value = "n/a";
            workbook.SaveAs(path);
        }

        File.WriteAllText(sidecarPath, """
            {
              "logicalName": "test-sample",
              "formatVersion": "v1"
            }
            """);

        return path;
    }
}
