namespace PoliticalPaths.Domain.Imports;

public class ImportRow
{
    public long Id { get; set; }
    public Guid ImportFileId { get; set; }
    public ImportFile ImportFile { get; set; } = null!;

    public string SheetName { get; set; } = null!;
    public int SheetIndex { get; set; }
    public int RowNumber { get; set; }
    public string RowHash { get; set; } = null!;
    public string RawPayloadJson { get; set; } = null!;
    public ImportRowStatus Status { get; set; } = ImportRowStatus.Pending;
    public DateTime ImportedAt { get; set; }
    public DateTime? TransformedAt { get; set; }
    public string? DomainEntityType { get; set; }
    public string? DomainEntityId { get; set; }

    public ICollection<TransformationError> Errors { get; set; } = [];
}
