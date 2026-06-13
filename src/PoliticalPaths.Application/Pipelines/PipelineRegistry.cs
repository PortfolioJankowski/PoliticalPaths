using PoliticalPaths.Application.Abstractions.Imports.Deserialization;
using PoliticalPaths.Shared.Paths;
using System.Text.Json;

namespace PoliticalPaths.Application.Pipelines;

public interface IPipelineRegistry
{
    ImportConfiguration GetImportConfiguration();
}

public sealed class PipelineRegistry : IPipelineRegistry
{
    private readonly ImportConfiguration _config;

    public PipelineRegistry()
    {
        var mappingsFilePath = Path.Combine(RepoPaths.SourceDataRoot(), "file-mappings.json");
        if (!File.Exists(mappingsFilePath))
        {
            throw new FileNotFoundException($"Mappings file not found at '{mappingsFilePath}'.");
        }
        
        var json = File.ReadAllText(mappingsFilePath);
        var config = JsonSerializer.Deserialize<ImportConfiguration>(json);
        
        if (config == null)
        {
            throw new InvalidOperationException($"Failed to deserialize import configuration from '{mappingsFilePath}'.");
        }
        _config = config;
    }

    ImportConfiguration IPipelineRegistry.GetImportConfiguration() => _config;
}
