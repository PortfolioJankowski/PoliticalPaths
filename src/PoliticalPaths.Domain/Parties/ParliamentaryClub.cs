using PoliticalPaths.Domain.Enums;
using PoliticalPaths.Domain.Mandates;

namespace PoliticalPaths.Domain.Parties;

public class ParliamentaryClub
{
    public Guid Id { get; set; }
    public Guid LegislativeTermId { get; set; }
    public CollegialBodyType Body { get; set; }
    public string Name { get; set; } = null!;
    public string NaturalKey { get; set; } = null!;

    public LegislativeTerm LegislativeTerm { get; set; } = null!;
    public ICollection<ClubMembership> Memberships { get; set; } = [];
}
