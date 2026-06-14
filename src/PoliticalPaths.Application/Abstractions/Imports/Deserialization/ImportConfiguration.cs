using System.Text.Json.Serialization;

namespace PoliticalPaths.Application.Abstractions.Imports.Deserialization;

public sealed class ImportConfiguration
{
    //pipelineKey -> List<ImportSourceDefinition>
    [property: JsonPropertyName("data")]
    public Dictionary<string, List<ImportSourceDefinition>> Data { get; init; } = new();
}
