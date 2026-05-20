using PoliticalPaths.Domain.Elections;

namespace PoliticalPaths.Domain.Results;

public class ElectoralListVoteResult
{
    public Guid Id { get; set; }
    public Guid ElectoralListId { get; set; }
    public Guid ElectionId { get; set; }
    public Guid ElectoralDistrictId { get; set; }
    public int? VotesReceived { get; set; }
    public decimal? VotePercent { get; set; }
    public int? SeatsWon { get; set; }
    public long? SourceImportRowId { get; set; }

    public ElectoralList ElectoralList { get; set; } = null!;
    public Election Election { get; set; } = null!;
    public ElectoralDistrict ElectoralDistrict { get; set; } = null!;
}
