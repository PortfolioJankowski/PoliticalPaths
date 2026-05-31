namespace PoliticalPaths.Application.Results;

public sealed record FileSyncResult(
    bool Skipped,
    int RowsRaw,
    int RowsTransformed,
    int RowsFailed,
    string FileName,
    DateTime StartedAt,
    DateTime FinishedAt);
