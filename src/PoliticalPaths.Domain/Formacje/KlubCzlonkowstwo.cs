namespace PoliticalPaths.Domain.Formacje;

public sealed class KlubCzlonkostwo
{
    public Guid Id { get; set; }

    public Guid PolitykId { get; set; }
    public Guid KlubId { get; set; }

    public Guid WyboryId { get; set; }

    public bool IsActive { get; set; }
}
