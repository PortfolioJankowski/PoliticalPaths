using PoliticalPaths.Domain.Candidacies;
using PoliticalPaths.Domain.Elections;
using PoliticalPaths.Domain.Politicians;

namespace PoliticalPaths.Domain.Mandates;

public class ElectionMandateAllocation
{
    public Guid Id { get; set; }
    public Guid ElectionId { get; set; }
    public Guid CandidacyId { get; set; }
    public Guid PoliticianId { get; set; }
    public Guid ElectoralDistrictId { get; set; }
    public Guid? ElectoralListId { get; set; }
    public int RankOnListByVotes { get; set; }
    public bool AllocatedSeat { get; set; }
    public Guid? MandateId { get; set; }
    public DateOnly? AllocationAnnouncedOn { get; set; }

    public Election Election { get; set; } = null!;
    public Candidacy Candidacy { get; set; } = null!;
    public Politician Politician { get; set; } = null!;
    public ElectoralDistrict ElectoralDistrict { get; set; } = null!;
    public ElectoralList? ElectoralList { get; set; }
    public Mandate? Mandate { get; set; }
}
