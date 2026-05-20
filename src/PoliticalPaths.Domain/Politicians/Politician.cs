namespace PoliticalPaths.Domain.Politicians;

public class Politician
{
    public Guid Id { get; set; }
    public string NormalizedName { get; set; } = null!;
    public string? DisplayName { get; set; }
    public DateOnly? BirthDate { get; set; }
    public string? PkwCandidateId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<PoliticianAlias> Aliases { get; set; } = [];
}
