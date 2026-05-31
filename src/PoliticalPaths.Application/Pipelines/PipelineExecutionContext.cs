using PoliticalPaths.Application.Deserialization;

namespace PoliticalPaths.Application.Pipelines;

public sealed record PipelineExecutionContext(
    string ElectionType,
    string ElectionYear,
    string PipelineKey,
    IReadOnlyList<ImportSourceDefinition> Sources);
