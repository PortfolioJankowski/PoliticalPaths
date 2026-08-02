using PoliticalPaths.Application.Abstractions.Imports;
using PoliticalPaths.Application.Abstractions.Imports.Deserialization;
using PoliticalPaths.Application.Results;
using PoliticalPaths.Domain.Imports;

namespace PoliticalPaths.Application.Imports.Transform;

public interface ITransformationExecutor
{
    Task<TransformFileResult> ExecuteAsync(
        string pipelineKey,
        ImportSourceDefinition source,
        ImportBatch batch,
        ImportFile file,
        IProgress<TransformationProgress>? progress = null,
        CancellationToken cancellationToken = default);

}
