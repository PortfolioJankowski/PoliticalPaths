using PoliticalPaths.Domain.Enums;

namespace PoliticalPaths.Domain.Geography;

public class TerritorialUnit
{
    public Guid Id { get; set; }
    public string TerytCode { get; set; } = null!;
    public string Name { get; set; } = null!;
    public TerritorialUnitLevel Level { get; set; }
    public string? ParentTerytCode { get; set; }
    public DateOnly? ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }

    public ICollection<ElectoralDistrictTerritory> DistrictTerritories { get; set; } = [];
}
