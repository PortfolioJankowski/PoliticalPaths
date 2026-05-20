using PoliticalPaths.Domain.Elections;
using PoliticalPaths.Domain.Enums;
using PoliticalPaths.Domain.Mandates;
using PoliticalPaths.Domain.Politicians;

namespace PoliticalPaths.Domain.Candidacies;

public class Candidacy
{
    public Guid Id { get; set; }
    public Guid PoliticianId { get; set; }
    public Guid ElectionId { get; set; }
    public ElectionProfile Profile { get; set; }
    public Guid? ElectoralDistrictId { get; set; }
    public Guid? ElectoralListId { get; set; }
    public Guid? ElectoralCommitteeId { get; set; }
    public int? ListPosition { get; set; }
    public string SourceFingerprint { get; set; } = null!;
    public long? SourceImportRowId { get; set; }

    public Politician Politician { get; set; } = null!;
    public Election Election { get; set; } = null!;
    public ElectoralDistrict? ElectoralDistrict { get; set; }
    public ElectoralList? ElectoralList { get; set; }
    public ElectoralCommittee? ElectoralCommittee { get; set; }
    public CandidacyVoteResult? VoteResult { get; set; }
    public ElectionMandateAllocation? MandateAllocation { get; set; }
}
