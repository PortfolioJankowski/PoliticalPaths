using PoliticalPaths.Domain.Enums;

namespace PoliticalPaths.Domain.Politicians;

public class IdentityMatchCandidate
{
    public Guid Id { get; set; }
    public Guid PoliticianId { get; set; }
    public Guid? MatchedPoliticianId { get; set; }
    public long? SourceImportRowId { get; set; }
    public decimal Score { get; set; }
    public IdentityMatchStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }

    public Politician Politician { get; set; } = null!;
    public Politician? MatchedPolitician { get; set; }
}
