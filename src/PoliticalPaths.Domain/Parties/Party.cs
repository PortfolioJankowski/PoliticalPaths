namespace PoliticalPaths.Domain.Parties;

public class Party
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? ShortName { get; set; }
    public string NaturalKey { get; set; } = null!;
}
