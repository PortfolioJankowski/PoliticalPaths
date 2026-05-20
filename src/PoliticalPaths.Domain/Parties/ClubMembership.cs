using PoliticalPaths.Domain.Politicians;

namespace PoliticalPaths.Domain.Parties;

public class ClubMembership
{
    public Guid Id { get; set; }
    public Guid ParliamentaryClubId { get; set; }
    public Guid PoliticianId { get; set; }
    public DateOnly ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
    public string? Source { get; set; }

    public ParliamentaryClub ParliamentaryClub { get; set; } = null!;
    public Politician Politician { get; set; } = null!;
}
