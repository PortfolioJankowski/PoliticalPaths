namespace PoliticalPaths.Domain.Imports;

public class ImportBatch
{
    public Guid Id { get; set; }

    /// <summary>
    /// Stabilny klucz pipeline (1 transformer = 1 batch). Np. "test-sample", "sejm-2023-listy".
    /// </summary>
    public string PipelineKey { get; set; } = null!;

    public ImportBatchStatus Status { get; set; } = ImportBatchStatus.Created;
    public string PrimarySourceType { get; set; }
    public int? ElectionYear { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? LastSyncedAt { get; set; }
    public string? TriggeredBy { get; set; }
    public string? Notes { get; set; }
    public Guid? SupersedesBatchId { get; set; }
    public ICollection<ImportFile> Files { get; set; } = [];

    public void StartBatch()
    {
        this.Status = ImportBatchStatus.Running;
        this.StartedAt = DateTime.UtcNow;
        this.LastSyncedAt = DateTime.UtcNow;
        this.CompletedAt = null;
    }

    public void Finish()
    {
        this.LastSyncedAt = DateTime.UtcNow;
        this.TriggeredBy = "sync";
    }
}
