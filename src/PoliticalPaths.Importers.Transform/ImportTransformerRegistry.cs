using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using PoliticalPaths.Application.Abstractions.Imports;

namespace PoliticalPaths.Importers.Transform;

public sealed class ImportTransformerRegistry(IServiceProvider serviceProvider) : IImportTransformerRegistry
{
    private readonly Dictionary<string, Type> _map = BuildMap();

    public IImportTransformer? Resolve(string pipelineKey)
    {
        return _map.TryGetValue(pipelineKey, out var type)
            ? (IImportTransformer)serviceProvider.GetRequiredService(type)
            : null;
    }

    private static Dictionary<string, Type> BuildMap()
    {
        var map = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        var assembly = typeof(ImportTransformerRegistry).Assembly;

        foreach (var type in assembly.GetTypes())
        {
            if (!typeof(IImportTransformer).IsAssignableFrom(type) || type is not { IsClass: true, IsAbstract: false })
                continue;

            var attr = type.GetCustomAttribute<ImportTransformerAttribute>();
            if (attr is null)
                continue;

            if (map.ContainsKey(attr.PipelineKey))
                throw new InvalidOperationException(
                    $"Duplicate transformer pipeline key '{attr.PipelineKey}': {map[attr.PipelineKey].Name} and {type.Name}");

            map[attr.PipelineKey] = type;
        }

        return map;
    }
}
