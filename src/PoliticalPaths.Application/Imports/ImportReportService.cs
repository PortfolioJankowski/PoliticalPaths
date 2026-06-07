using RazorLight;
using PoliticalPaths.Application.Abstractions.Imports;
using PoliticalPaths.Application.Results;
using PoliticalPaths.Shared.Paths;
using System.Text;

namespace PoliticalPaths.Application.Imports;

public sealed class ImportReportService : IImportReportService
{
    private readonly IRazorLightEngine _engine;

    public ImportReportService()
    {
        _engine = new RazorLightEngineBuilder()
            .UseEmbeddedResourcesProject(typeof(ImportReportService))
            .UseMemoryCachingProvider()
            .Build();
    }

    public async Task GenerateReportAsync(ImportSyncResult result, CancellationToken ct = default)
    {
        var template = GetTemplate();
        var html = await _engine.CompileRenderStringAsync("ImportReport", template, result);

        var reportsDir = Path.Combine(RepoPaths.SourceDataRoot(), "reports");
        if (!Directory.Exists(reportsDir)) Directory.CreateDirectory(reportsDir);

        var fileName = $"report_{DateTime.Now:yyyyMMdd_HHmmss}.html";
        var filePath = Path.Combine(reportsDir, fileName);

        await File.WriteAllTextAsync(filePath, html, Encoding.UTF8, ct);
    }

    private string GetTemplate()
    {
        return @"
<!DOCTYPE html>
<html lang=""pl"">
<head>
    <meta charset=""UTF-8"">
    <title>Raport Importu - PoliticalPaths</title>
    <style>
        body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f7f6; color: #333; margin: 0; padding: 20px; }
        .container { max-width: 1000px; margin: auto; background: #fff; padding: 30px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }
        h1 { color: #2c3e50; border-bottom: 2px solid #3498db; padding-bottom: 10px; }
        .summary-cards { display: flex; gap: 20px; margin-bottom: 30px; }
        .card { flex: 1; padding: 20px; border-radius: 8px; color: #fff; text-align: center; }
        .card.blue { background-color: #3498db; }
        .card.green { background-color: #2ecc71; }
        .card.red { background-color: #e74c3c; }
        .card.orange { background-color: #f39c12; }
        .card .value { font-size: 2.5em; font-weight: bold; }
        .card .label { font-size: 1.1em; opacity: 0.9; }
        table { width: 100%; border-collapse: collapse; margin-top: 20px; }
        th, td { text-align: left; padding: 12px; border-bottom: 1px solid #ddd; }
        th { background-color: #f8f9fa; color: #2c3e50; }
        tr:hover { background-color: #f1f1f1; }
        .status { font-weight: bold; }
        .status.success { color: #2ecc71; }
        .status.warning { color: #f39c12; }
    </style>
</head>
<body>
    <div class=""container"">
        <h1>Raport z Synchronizacji Danych</h1>
        <p>Data wykonania: @DateTime.Now.ToString(""G"")</p>

        <div class=""summary-cards"">
            <div class=""card blue"">
                <div class=""value"">@Model.PipelinesCount</div>
                <div class=""label"">Pipelines</div>
            </div>
            <div class=""card green"">
                <div class=""value"">@Model.FilesImported</div>
                <div class=""label"">Pliki Zaimportowane</div>
            </div>
            <div class=""card orange"">
                <div class=""value"">@Model.RowsTransformed</div>
                <div class=""label"">Wiersze OK</div>
            </div>
            <div class=""card red"">
                <div class=""value"">@Model.FilesSkipped</div>
                <div class=""label"">Pliki Pominieęte</div>
            </div>
        </div>

        <h2>Szczegóły Pipeline'ów</h2>
        <table>
            <thead>
                <tr>
                    <th>Pipeline</th>
                    <th>Pliki OK</th>
                    <th>Wiersze Przetworzone</th>
                    <th>Wiersze Błędy</th>
                    <th>Status</th>
                </tr>
            </thead>
            <tbody>
                @foreach(var p in Model.Summaries) {
                <tr>
                    <td>@p.PipelineKey</td>
                    <td>@p.FilesImported</td>
                    <td>@p.RowsTransformed</td>
                    <td>@p.RowsFailed</td>
                    <td>
                        @if(p.RowsFailed == 0) {
                            <span class=""status success"">SUKCES</span>
                        } else {
                            <span class=""status warning"">OSTRZEŻENIA</span>
                        }
                    </td>
                </tr>
                }
            </tbody>
        </table>
    </div>
</body>
</html>";
    }
}
