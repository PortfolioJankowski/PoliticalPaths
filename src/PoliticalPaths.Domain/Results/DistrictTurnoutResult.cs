using PoliticalPaths.Domain.Elections;

namespace PoliticalPaths.Domain.Results;

public class DistrictTurnoutResult
{
    public Guid Id { get; set; }
    public Guid ElectionId { get; set; }
    public Guid ElectoralDistrictId { get; set; }
    public int? BallotsIssued { get; set; }
    public int? VotesValid { get; set; }
    public int? VotesInvalid { get; set; }
    public decimal? TurnoutPercent { get; set; }
    public long? SourceImportRowId { get; set; }

    public Election Election { get; set; } = null!;
    public ElectoralDistrict ElectoralDistrict { get; set; } = null!;
}
