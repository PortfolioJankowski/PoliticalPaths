using PoliticalPaths.Domain.Imports;

namespace PoliticalPaths.Application.Abstractions.Imports;

public sealed record PipelineDefinition(
    string PipelineKey,
    IReadOnlyList<string> LogicalNames,
    DataSourceType DataSourceType);
