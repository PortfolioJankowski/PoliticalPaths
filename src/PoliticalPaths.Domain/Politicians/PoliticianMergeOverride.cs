namespace PoliticalPaths.Domain.Politicians;

public class PoliticianMergeOverride
{
    public Guid Id { get; set; }
    public Guid SourcePoliticianId { get; set; }
    public Guid TargetPoliticianId { get; set; }
    public string? Reason { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    public Politician SourcePolitician { get; set; } = null!;
    public Politician TargetPolitician { get; set; } = null!;
}
