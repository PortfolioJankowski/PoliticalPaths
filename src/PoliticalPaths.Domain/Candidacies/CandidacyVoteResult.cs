using PoliticalPaths.Domain.Elections;

namespace PoliticalPaths.Domain.Candidacies;

public class CandidacyVoteResult
{
    public Guid Id { get; set; }
    public Guid CandidacyId { get; set; }
    public Guid ElectionId { get; set; }
    public Guid ElectoralDistrictId { get; set; }
    public int? VotesReceived { get; set; }
    public int? PreferentialVotes { get; set; }
    public decimal? VotePercent { get; set; }
    public bool? Elected { get; set; }
    public long? SourceImportRowId { get; set; }

    public Candidacy Candidacy { get; set; } = null!;
    public Election Election { get; set; } = null!;
    public ElectoralDistrict ElectoralDistrict { get; set; } = null!;
}
