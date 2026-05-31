using PoliticalPaths.Application.Deserialization;
using System.Text.Json.Serialization;

namespace PoliticalPaths.Application.Abstractions.Imports;

public sealed class ImportConfiguration
{
    [property: JsonPropertyName("data")]
    public Dictionary<string, Dictionary<string, List<ImportSourceDefinition>>> Data { get; init; } = new();
}
