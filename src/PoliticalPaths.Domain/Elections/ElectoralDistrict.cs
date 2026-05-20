using PoliticalPaths.Domain.Enums;
using PoliticalPaths.Domain.Geography;

namespace PoliticalPaths.Domain.Elections;

public class ElectoralDistrict
{
    public Guid Id { get; set; }
    public Guid ElectionId { get; set; }
    public ElectoralChamber Chamber { get; set; }
    public int DistrictNumber { get; set; }
    public string? Name { get; set; }
    public string NaturalKey { get; set; } = null!;

    public Election Election { get; set; } = null!;
    public ICollection<ElectoralDistrictSnapshot> Snapshots { get; set; } = [];
    public ICollection<ElectoralDistrictTerritory> Territories { get; set; } = [];
    public ICollection<ElectoralList> Lists { get; set; } = [];
}
