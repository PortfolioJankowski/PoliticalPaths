using PoliticalPaths.Application.Abstractions.Imports.Deserialization;

namespace PoliticalPaths.Application.Pipelines;

public sealed record PipelineExecutionContext(
    string PipelineKey,
    IReadOnlyList<ImportSourceDefinition> Sources);
