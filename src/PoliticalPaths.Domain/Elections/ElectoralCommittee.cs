using PoliticalPaths.Domain.Enums;
using PoliticalPaths.Domain.Parties;

namespace PoliticalPaths.Domain.Elections;

public class ElectoralCommittee
{
    public Guid Id { get; set; }
    public Guid ElectionId { get; set; }
    public string Name { get; set; } = null!;
    public string? ShortName { get; set; }
    public ElectoralCommitteeType Type { get; set; }
    public Guid? PartyId { get; set; }
    public string NaturalKey { get; set; } = null!;

    public Election Election { get; set; } = null!;
    public Party? Party { get; set; }
    public ICollection<ElectoralList> Lists { get; set; } = [];
}
