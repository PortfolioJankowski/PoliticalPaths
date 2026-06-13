namespace PoliticalPaths.Domain.Imports;

public class ImportFile
{
    public Guid Id { get; set; }
    public Guid ImportBatchId { get; set; }
    public ImportBatch ImportBatch { get; set; } = null!;

    public string[] LogicalNames { get; set; } = [];
    public string StoragePath { get; set; } = null!;
    public string Sha256 { get; set; } = null!;
    public long FileSizeBytes { get; set; }
    public string ContentType { get; set; } = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    public string DataSourceType { get; set; } = null!;
    public string FormatVersion { get; set; } = "v1";

    public ImportFileStatus Status { get; set; } = ImportFileStatus.Discovered;
    public int TotalRows { get; set; }
    public int TransformedRows { get; set; }
    public int FailedRows { get; set; }
    public int WarningCount { get; set; }
    public long? LastProcessedRowId { get; set; }
    public string? LogFilePath { get; set; }
    public DateTime? RawImportStartedAt { get; set; }
    public DateTime? RawImportCompletedAt { get; set; }

    public ICollection<ImportRow> Rows { get; set; } = [];

    public void EreaseData()
    {
        this.Rows.Clear();
        this.TotalRows = 0;
        this.TransformedRows = 0;
        this.FailedRows = 0;
        this.WarningCount = 0;
        this.LastProcessedRowId = null;
        this.LogFilePath = null;
        this.RawImportStartedAt = null;
        this.RawImportCompletedAt = null;
        this.Status = ImportFileStatus.Discovered;
    }


}
