namespace PoliticalPaths.Domain.Formacje;

public sealed class KlubyCzlonkowstwo
{
    public Guid Id { get; set; }
    public Guid PolitykId { get; set; }
    public Guid KlubId { get; set; }
    public DateOnly DataDolaczenia { get; set; }
    public DateOnly? DataOdejscia { get; set; }
    public string? Powod { get; set; }
}
