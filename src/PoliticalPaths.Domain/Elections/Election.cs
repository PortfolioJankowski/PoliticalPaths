using PoliticalPaths.Domain.Enums;
using PoliticalPaths.Domain.Geography;
using PoliticalPaths.Domain.Mandates;

namespace PoliticalPaths.Domain.Elections;

public class Election
{
    public Guid Id { get; set; }
    public int Year { get; set; }
    public ElectoralChamber Chamber { get; set; }
    public ElectionScope Scope { get; set; }
    public ElectionProfile Profile { get; set; }
    public ElectionKind Kind { get; set; } = ElectionKind.General;
    public Guid? VoivodeshipTerritorialUnitId { get; set; }
    public DateOnly? ElectionDate { get; set; }
    public string NaturalKey { get; set; } = null!;
    public Guid? LegislativeTermId { get; set; }
    public Guid? ReplacesMandateId { get; set; }
    public Guid? ParentLegislativeTermId { get; set; }

    public TerritorialUnit? VoivodeshipTerritorialUnit { get; set; }
    public LegislativeTerm? LegislativeTerm { get; set; }
    public LegislativeTerm? ParentLegislativeTerm { get; set; }

    public ICollection<ElectoralDistrict> Districts { get; set; } = [];
    public ICollection<ElectoralCommittee> Committees { get; set; } = [];
    public ICollection<ElectoralList> Lists { get; set; } = [];
}
