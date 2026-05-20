namespace PoliticalPaths.Application.Abstractions.Imports;

public interface IPipelineRegistry
{
    IReadOnlyList<PipelineDefinition> GetAll();

    PipelineDefinition GetByKey(string pipelineKey);

    bool TryGetByLogicalName(string logicalName, out PipelineDefinition? pipeline);
}
