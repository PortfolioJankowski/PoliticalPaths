using System.Reflection;
using PoliticalPaths.Application.Abstractions.Imports;
using PoliticalPaths.Domain.Imports;

namespace PoliticalPaths.Importers.Raw;

public sealed class PipelineRegistry : IPipelineRegistry
{
    private readonly IReadOnlyList<PipelineDefinition> _pipelines;
    private readonly Dictionary<string, PipelineDefinition> _byKey;
    private readonly Dictionary<string, PipelineDefinition> _byLogicalName;

    public PipelineRegistry()
    {
        _pipelines = BuildPipelines();
        _byKey = _pipelines.ToDictionary(p => p.PipelineKey, StringComparer.OrdinalIgnoreCase);
        _byLogicalName = new Dictionary<string, PipelineDefinition>(StringComparer.OrdinalIgnoreCase);

        foreach (var pipeline in _pipelines)
        {
            foreach (var name in pipeline.LogicalNames)
            {
                if (_byLogicalName.ContainsKey(name))
                    throw new InvalidOperationException(
                        $"Logical name '{name}' is registered in multiple pipelines.");

                _byLogicalName[name] = pipeline;
            }
        }
    }

    public IReadOnlyList<PipelineDefinition> GetAll() => _pipelines;

    public PipelineDefinition GetByKey(string pipelineKey)
    {
        if (!_byKey.TryGetValue(pipelineKey, out var pipeline))
            throw new InvalidOperationException(
                $"Unknown pipeline key '{pipelineKey}'. Registered: {string.Join(", ", _byKey.Keys.OrderBy(x => x))}");

        return pipeline;
    }

    public bool TryGetByLogicalName(string logicalName, out PipelineDefinition? pipeline)
    {
        if (_byLogicalName.TryGetValue(logicalName, out var found))
        {
            pipeline = found;
            return true;
        }

        pipeline = null;
        return false;
    }

    private static IReadOnlyList<PipelineDefinition> BuildPipelines()
    {
        var map = new Dictionary<string, (HashSet<string> Names, DataSourceType Type)>(StringComparer.OrdinalIgnoreCase);
        var assembly = typeof(PipelineRegistry).Assembly;

        foreach (var type in assembly.GetTypes())
        {
            if (!typeof(IRawExcelImporter).IsAssignableFrom(type) || type is not { IsClass: true, IsAbstract: false })
                continue;

            var attr = type.GetCustomAttribute<RawImporterAttribute>();
            if (attr is null)
                continue;

            if (!map.TryGetValue(attr.PipelineKey, out var entry))
            {
                entry = (new HashSet<string>(StringComparer.OrdinalIgnoreCase), attr.DataSourceType);
                map[attr.PipelineKey] = entry;
            }

            foreach (var name in attr.LogicalNames)
                entry.Names.Add(name);
        }

        if (map.Count == 0)
            throw new InvalidOperationException("No pipelines registered. Add [RawImporter] to at least one IRawExcelImporter.");

        return map
            .OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
            .Select(kv => new PipelineDefinition(kv.Key, kv.Value.Names.OrderBy(x => x).ToList(), kv.Value.Type))
            .ToList();
    }
}
