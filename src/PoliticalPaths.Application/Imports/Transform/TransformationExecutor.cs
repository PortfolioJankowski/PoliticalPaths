using PoliticalPaths.Application.Abstractions.Imports;
using PoliticalPaths.Application.Pipelines;
using PoliticalPaths.Application.Results;
using PoliticalPaths.Domain.Imports;

namespace PoliticalPaths.Application.Imports.Transform;

public sealed class TransformationExecutor(
IEnumerable<IImportTransformer> transformers)
: ITransformationExecutor
{
    private readonly Dictionary<string, IImportTransformer> _transformers =
        transformers.ToDictionary(
            x => x.PipelineKey,
            StringComparer.OrdinalIgnoreCase);

    public async Task<TransformFileResult> ExecuteAsync(
        PipelineExecutionContext context,
        ImportBatch batch,
        ImportFile file,
        CancellationToken cancellationToken)
    {
        if (!_transformers.TryGetValue(
                context.PipelineKey,
                out var transformer))
        {
            return TransformFileResult.Skip(
                $"No transformer registered for '{context.PipelineKey}'.");
        }

        return await transformer.TransformFileAsync(
            file,
            context,
            cancellationToken);
    }
}
