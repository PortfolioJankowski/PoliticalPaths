namespace PoliticalPaths.Domain.Formacje;

public sealed class PartiaCzlonkostwo
{
    public Guid Id { get; set; }
    public Guid PolitykId { get; set; }
    public Guid PartiaId { get; set; }
    public Guid WyboryId { get; set; }
    public bool IsActive { get; set; }
}
