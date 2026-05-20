namespace PoliticalPaths.Domain.Politicians;

public class PoliticianAlias
{
    public Guid Id { get; set; }
    public Guid PoliticianId { get; set; }
    public string AliasName { get; set; } = null!;
    public string NormalizedAlias { get; set; } = null!;
    public DateOnly? ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
    public string? Source { get; set; }

    public Politician Politician { get; set; } = null!;
}
