using RazorLight;
using Microsoft.Extensions.Configuration;
using PoliticalPaths.Application.Abstractions.Imports;
using PoliticalPaths.Application.Results;
using PoliticalPaths.Shared.Paths;
using System.Text;

namespace PoliticalPaths.Application.Imports;

public sealed class ImportReportService : IImportReportService
{
    private readonly IRazorLightEngine _engine;

    private readonly string _reportsDirectory;

    public ImportReportService(IConfiguration configuration)
    {
        _engine = new RazorLightEngineBuilder()
            .UseEmbeddedResourcesProject(typeof(ImportReportService))
            .SetOperatingAssembly(typeof(ImportReportService).Assembly)
            .UseMemoryCachingProvider()
            .EnableDebugMode(true)
            .Build();

        _reportsDirectory = configuration["Import:ReportsPath"]
            ?? Path.Combine(RepoPaths.SourceDataRoot(), "reports");
    }

    public async Task GenerateReportAsync(ImportSyncResult result, CancellationToken ct = default)
    {
        try
        {
            var html = await _engine.CompileRenderAsync("Templates.ImportReport", result);
            if (!Directory.Exists(_reportsDirectory)) Directory.CreateDirectory(_reportsDirectory);

            var fileName = $"report_{DateTime.Now:yyyyMMdd_HHmmss}.html";
            var filePath = Path.Combine(_reportsDirectory, fileName);

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
