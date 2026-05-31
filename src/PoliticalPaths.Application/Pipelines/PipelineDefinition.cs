using PoliticalPaths.Application.Deserialization;

namespace PoliticalPaths.Application.Pipelines;

public sealed record PipelineDefinition(
    string PipelineKey,
    IReadOnlyList<ImportSourceDefinition> Sources);


