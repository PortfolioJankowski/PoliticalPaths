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
            .SetOperatingAssembly(typeof(ImportReportService).Assembly)
            .UseMemoryCachingProvider()
            .EnableDebugMode(true)
            .Build();
    }

    public async Task GenerateReportAsync(ImportSyncResult result, CancellationToken ct = default)
    {
        try
        {
            var html = await _engine.CompileRenderAsync("Templates.ImportReport", result);
            var reportsDir = Path.Combine(RepoPaths.SourceDataRoot(), "reports");
            if (!Directory.Exists(reportsDir)) Directory.CreateDirectory(reportsDir);

            var fileName = $"report_{DateTime.Now:yyyyMMdd_HHmmss}.html";
            var filePath = Path.Combine(reportsDir, fileName);

            await File.WriteAllTextAsync(filePath, html, Encoding.UTF8, ct);
        }
        catch (RazorLight.TemplateNotFoundException templateNotFoundException)
        {
            var projectKeys = string.Join(" -- known project key\n", templateNotFoundException.KnownProjectTemplateKeys);
            var dynamicKeys = string.Join(" -- known dynamic key\n", templateNotFoundException.KnownDynamicTemplateKeys);

            Console.WriteLine($"{projectKeys}\n\n{dynamicKeys}");

            throw;
        }
    }
}
