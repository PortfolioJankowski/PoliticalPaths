namespace PoliticalPaths.Domain.Imports;

public class ImportJob
{
    public Guid Id { get; set; }
    public Guid? ImportBatchId { get; set; }
    public ImportBatch? ImportBatch { get; set; }
    public ImportJobType JobType { get; set; }
    public ImportJobStatus Status { get; set; } = ImportJobStatus.Pending;
    public int Attempt { get; set; }
    public DateTime? NextRetryAt { get; set; }
    public string? LastError { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
