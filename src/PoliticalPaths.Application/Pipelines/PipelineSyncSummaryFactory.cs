using PoliticalPaths.Application.Abstractions.Imports;
using PoliticalPaths.Application.Results;

namespace PoliticalPaths.Application.Pipelines;

public static class PipelineSyncSummaryFactory
{
    public static PipelineSyncSummary Create(
     string pipelineKey,
     Guid batchId,
     IReadOnlyList<FileSyncResult> fileSummaries,
     bool transformSkippedNoTransformer)
    {
        var filesImported = fileSummaries.Count(x => !x.Skipped);
        var filesSkipped = fileSummaries.Count(x => x.Skipped);
        var rowsRaw = fileSummaries.Sum(x => x.RowsRaw);
        var rowsTransformed = fileSummaries.Sum(x => x.RowsTransformed);
        var rowsFailed = fileSummaries.Sum(x => x.RowsFailed);
        return new PipelineSyncSummary(
            PipelineKey: pipelineKey,
            BatchId: batchId,
            FilesImported: filesImported,
            FilesSkipped: filesSkipped,
            RowsRaw: rowsRaw,
            RowsTransformed: rowsTransformed,
            RowsFailed: rowsFailed,
            TransformSkippedNoTransformer: transformSkippedNoTransformer);
    }
}
