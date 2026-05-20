using PoliticalPaths.Domain.Imports;

namespace PoliticalPaths.Application.Abstractions.Imports;

public interface IImportTransformer
{
    string PipelineKey { get; }

    Task<TransformFileResult> TransformFileAsync(
        ImportFile file,
        IReadOnlyList<ImportRow> rows,
        CancellationToken cancellationToken = default);
}

public sealed record TransformFileResult(
    int RowsTransformed,
    int RowsFailed,
    int Warnings);
