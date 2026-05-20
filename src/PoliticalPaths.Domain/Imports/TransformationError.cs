namespace PoliticalPaths.Domain.Imports;

public class TransformationError
{
    public long Id { get; set; }
    public long ImportRowId { get; set; }
    public ImportRow ImportRow { get; set; } = null!;

    public string StepName { get; set; } = null!;
    public TransformationSeverity Severity { get; set; }
    public string ErrorCode { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string? FieldName { get; set; }
    public string? RawValue { get; set; }
    public string? DetailsJson { get; set; }
    public DateTime CreatedAt { get; set; }
}
