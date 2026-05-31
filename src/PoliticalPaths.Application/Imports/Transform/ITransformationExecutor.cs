using PoliticalPaths.Application.Pipelines;
using PoliticalPaths.Application.Results;
using PoliticalPaths.Domain.Imports;

namespace PoliticalPaths.Application.Imports.Transform;

public interface ITransformationExecutor
{
    Task<TransformFileResult> ExecuteAsync(
        PipelineExecutionContext context,
        ImportBatch batch,
        ImportFile file,
        CancellationToken cancellationToken);
}
