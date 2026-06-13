namespace PoliticalPaths.Application.Abstractions.Imports.Deserialization;

public sealed class ImportConfiguration
{
    //pipelineKey -> List<ImportSourceDefinition>
    public Dictionary<string, List<ImportSourceDefinition>> Data { get; init; } = new();
}
