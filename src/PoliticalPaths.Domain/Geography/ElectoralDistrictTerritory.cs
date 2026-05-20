using PoliticalPaths.Domain.Enums;
using PoliticalPaths.Domain.Elections;

namespace PoliticalPaths.Domain.Geography;

public class ElectoralDistrictTerritory
{
    public Guid Id { get; set; }
    public Guid ElectoralDistrictId { get; set; }
    public Guid TerritorialUnitId { get; set; }
    public TerritoryCoverageType? CoverageType { get; set; }

    public ElectoralDistrict ElectoralDistrict { get; set; } = null!;
    public TerritorialUnit TerritorialUnit { get; set; } = null!;
}
