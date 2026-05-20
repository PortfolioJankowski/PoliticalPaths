using PoliticalPaths.Domain.Enums;

namespace PoliticalPaths.Domain.Mapping;

public class ManualMapping
{
    public Guid Id { get; set; }
    public ManualMappingCategory Category { get; set; }
    public string SourceKey { get; set; } = null!;
    public string TargetEntityType { get; set; } = null!;
    public Guid TargetEntityId { get; set; }
    public string? Notes { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}
