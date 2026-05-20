using PoliticalPaths.Domain.Enums;
using PoliticalPaths.Domain.Elections;
using PoliticalPaths.Domain.Geography;

namespace PoliticalPaths.Domain.Mandates;

public class LegislativeTerm
{
    public Guid Id { get; set; }
    public CollegialBodyType Body { get; set; }
    public int TermNumber { get; set; }
    public DateOnly? ConstituentSessionDate { get; set; }
    public DateOnly? DissolvedOn { get; set; }
    public Guid FoundingElectionId { get; set; }
    public Guid? VoivodeshipTerritorialUnitId { get; set; }
    public string NaturalKey { get; set; } = null!;

    public Election FoundingElection { get; set; } = null!;
    public TerritorialUnit? VoivodeshipTerritorialUnit { get; set; }
    public ICollection<Mandate> Mandates { get; set; } = [];
}
