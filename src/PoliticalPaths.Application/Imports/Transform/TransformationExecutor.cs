using PoliticalPaths.Application.Abstractions.Imports;
using PoliticalPaths.Application.Abstractions.Imports.Deserialization;
using PoliticalPaths.Application.Results;
using PoliticalPaths.Domain.Imports;
namespace PoliticalPaths.Application.Imports.Transform;

public sealed class TransformationExecutor(
    IEnumerable<IImportTransformer> transformers,
    IExcelProcessor excelProcessor)
    : ITransformationExecutor
{
    private readonly Dictionary<string, IImportTransformer> _transformers =
        transformers.ToDictionary(
            x => x.PipelineKey,
            StringComparer.OrdinalIgnoreCase);

    public async Task<TransformFileResult> ExecuteAsync(
        string pipelineKey,
        ImportSourceDefinition source,
        ImportBatch batch,
        ImportFile file,
        IProgress<TransformationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (!_transformers.TryGetValue(
                pipelineKey,
                out var transformer))
        {
            return TransformFileResult.Skip(
                $"No transformer registered for '{pipelineKey}'.");
        }

        var workbook = excelProcessor.GetWorkbook(file.StoragePath);

        return await transformer.TransformFileAsync(
            file,
            workbook,
            pipelineKey, source, progress, cancellationToken);
    }
}
