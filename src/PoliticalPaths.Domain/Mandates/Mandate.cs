using PoliticalPaths.Domain.Candidacies;
using PoliticalPaths.Domain.Elections;
using PoliticalPaths.Domain.Enums;
using PoliticalPaths.Domain.Politicians;

namespace PoliticalPaths.Domain.Mandates;

public class Mandate
{
    public Guid Id { get; set; }
    public Guid LegislativeTermId { get; set; }
    public Guid PoliticianId { get; set; }
    public CollegialBodyType Body { get; set; }
    public Guid? ElectoralDistrictId { get; set; }
    public Guid? ElectoralListId { get; set; }
    public Guid? ElectoralCommitteeId { get; set; }
    public Guid? OriginatingCandidacyId { get; set; }
    public Guid? OriginatingElectionId { get; set; }
    public MandateAcquisitionType AcquisitionType { get; set; }
    public MandateStatus Status { get; set; }
    public DateOnly ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
    public MandateTerminationReason? TerminationReason { get; set; }
    public string? TerminationNote { get; set; }
    public Guid? PredecessorMandateId { get; set; }
    public int? SuccessorPriorityOnList { get; set; }

    public LegislativeTerm LegislativeTerm { get; set; } = null!;
    public Politician Politician { get; set; } = null!;
    public ElectoralDistrict? ElectoralDistrict { get; set; }
    public ElectoralList? ElectoralList { get; set; }
    public ElectoralCommittee? ElectoralCommittee { get; set; }
    public Candidacy? OriginatingCandidacy { get; set; }
    public Election? OriginatingElection { get; set; }
    public Mandate? PredecessorMandate { get; set; }
    public ICollection<Mandate> SuccessorMandates { get; set; } = [];
    public ICollection<MandateEvent> Events { get; set; } = [];
}
