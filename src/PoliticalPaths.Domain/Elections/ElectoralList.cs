using PoliticalPaths.Domain.Parties;

namespace PoliticalPaths.Domain.Elections;

public class ElectoralList
{
    public Guid Id { get; set; }
    public Guid ElectionId { get; set; }
    public Guid ElectoralDistrictId { get; set; }
    public Guid ElectoralCommitteeId { get; set; }
    public int ListNumber { get; set; }
    public Guid? PartyId { get; set; }
    public string NaturalKey { get; set; } = null!;

    public Election Election { get; set; } = null!;
    public ElectoralDistrict ElectoralDistrict { get; set; } = null!;
    public ElectoralCommittee ElectoralCommittee { get; set; } = null!;
    public Party? Party { get; set; }
}
