using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using PoliticalPaths.Application.Abstractions.Imports;

namespace PoliticalPaths.Importers.Raw;

public sealed class RawImporterRegistry(IServiceProvider serviceProvider) : IRawImporterRegistry
{
    private readonly Dictionary<string, Type> _map = BuildMap();

    public IRawExcelImporter Resolve(string logicalName)
    {
        if (!_map.TryGetValue(logicalName, out var type))
            throw new InvalidOperationException(
                $"No RAW importer registered for logical name '{logicalName}'. Registered: {string.Join(", ", _map.Keys.OrderBy(x => x))}");

        return (IRawExcelImporter)serviceProvider.GetRequiredService(type);
    }

    private static Dictionary<string, Type> BuildMap()
    {
        var map = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        var assembly = typeof(RawImporterRegistry).Assembly;

        foreach (var type in assembly.GetTypes())
        {
            if (!typeof(IRawExcelImporter).IsAssignableFrom(type) || type is not { IsClass: true, IsAbstract: false })
                continue;

            var attr = type.GetCustomAttribute<RawImporterAttribute>();
            if (attr?.LogicalNames is not { Length: > 0 })
                continue;

            foreach (var name in attr.LogicalNames)
            {
                if (map.ContainsKey(name))
                    throw new InvalidOperationException(
                        $"Duplicate RAW importer logical name '{name}': {map[name].Name} and {type.Name}");

                map[name] = type;
            }
        }

        return map;
    }
}
