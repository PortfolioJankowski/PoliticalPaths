using PoliticalPaths.Application.Pipelines;
using PoliticalPaths.Application.Results;
using PoliticalPaths.Domain.Imports;

namespace PoliticalPaths.Application.Abstractions.Imports;

public interface IImportTransformer
{
    string PipelineKey { get; }

    Task<TransformFileResult> TransformFileAsync(
        ImportFile file,
        PipelineExecutionContext context,
        CancellationToken cancellationToken = default);
}
