using PoliticalPaths.Domain.Politicians;

namespace PoliticalPaths.Domain.Parties;

public class PartyAffiliation
{
    public Guid Id { get; set; }
    public Guid PoliticianId { get; set; }
    public Guid PartyId { get; set; }
    public DateOnly ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
    public string? Source { get; set; }

    public Politician Politician { get; set; } = null!;
    public Party Party { get; set; } = null!;
}
