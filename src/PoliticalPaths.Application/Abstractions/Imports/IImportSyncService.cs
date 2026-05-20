namespace PoliticalPaths.Application.Abstractions.Imports;

public interface IImportSyncService
{
    Task<ImportSyncResult> SyncAllAsync(ImportSyncOptions options, CancellationToken cancellationToken = default);
}

public sealed record ImportSyncOptions(
    string InboxRoot,
    bool SeedIfEmpty = true,
    bool ForceReimport = false);

public sealed record ImportSyncResult(
    int PipelinesProcessed,
    int FilesImported,
    int FilesSkipped,
    int TotalRowsRaw,
    int TotalRowsTransformed,
    IReadOnlyList<PipelineSyncSummary> Pipelines);

public sealed record PipelineSyncSummary(
    string PipelineKey,
    Guid BatchId,
    int FilesImported,
    int FilesSkipped,
    int RowsRaw,
    int RowsTransformed,
    int RowsFailed,
    bool TransformSkippedNoTransformer);

public sealed record FileSyncSummary(
    string FileName,
    string LogicalName,
    bool Skipped,
    int RowsRaw,
    int RowsTransformed,
    int RowsFailed,
    string? Message);
