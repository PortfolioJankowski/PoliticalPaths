using PoliticalPaths.Domain.Enums;
using PoliticalPaths.Domain.Elections;

namespace PoliticalPaths.Domain.Mandates;

public class MandateEvent
{
    public long Id { get; set; }
    public Guid MandateId { get; set; }
    public MandateEventType Type { get; set; }
    public DateTime OccurredAt { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public MandateTerminationReason? Reason { get; set; }
    public Guid? RelatedMandateId { get; set; }
    public Guid? RelatedElectionId { get; set; }
    public string? SourceUrl { get; set; }
    public string? SourceDocumentRef { get; set; }
    public long? SourceImportRowId { get; set; }
    public string? DetailsJson { get; set; }

    public Mandate Mandate { get; set; } = null!;
    public Mandate? RelatedMandate { get; set; }
    public Election? RelatedElection { get; set; }
}
