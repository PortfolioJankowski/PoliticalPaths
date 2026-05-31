using PoliticalPaths.Application.Abstractions.Imports;
using PoliticalPaths.Application.Deserialization;
using PoliticalPaths.Shared.Paths;
using System.Text.Json;

namespace PoliticalPaths.Application.Pipelines;

public interface IPipelineRegistry
{
    ImportConfiguration GetImportConfiguration();
    PipelineDefinition GetByPipelineKey(string pipelineKey);

}

public sealed class PipelineRegistry : IPipelineRegistry
{
    private readonly IReadOnlyList<PipelineDefinition> _pipelines;
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

        _pipelines = BuildPipelines(config);
    }

    ImportConfiguration IPipelineRegistry.GetImportConfiguration() => _config;

    public PipelineDefinition GetByPipelineKey(string pipelineKey)
    {
        var found = _pipelines.FirstOrDefault(p => p.PipelineKey.Equals(pipelineKey, StringComparison.OrdinalIgnoreCase));
        if (found == null)
        {
            throw new InvalidOperationException($"Unknown pipeline key '{pipelineKey}'.");
        }

        return found;
    }

    private static IReadOnlyList<PipelineDefinition> BuildPipelines(
    ImportConfiguration config)
    {
        var allSources = new List<ImportSourceDefinition>();

        foreach (var category in config.Data)
        {
            foreach (var year in category.Value)
            {
                foreach (var source in year.Value)
                {
                    allSources.Add(source);
                }
            }
        }

        return allSources
            .GroupBy(x => x.Pipeline)
            .Select(g => new PipelineDefinition(
                PipelineKey: g.Key,
                Sources: g.ToList()))
            .ToList();
    }

}
