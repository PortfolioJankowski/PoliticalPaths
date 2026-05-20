namespace PoliticalPaths.Domain.Elections;

public class ElectoralDistrictSnapshot
{
    public Guid Id { get; set; }
    public Guid ElectoralDistrictId { get; set; }
    public Guid ElectionId { get; set; }
    public int? Population { get; set; }
    public int? EligibleVoters { get; set; }
    public int? RegisteredVoters { get; set; }
    public int? SeatsAllocated { get; set; }
    public DateOnly? StatisticsDate { get; set; }
    public long? SourceImportRowId { get; set; }
    public DateTime CreatedAt { get; set; }

    public ElectoralDistrict ElectoralDistrict { get; set; } = null!;
    public Election Election { get; set; } = null!;
}
