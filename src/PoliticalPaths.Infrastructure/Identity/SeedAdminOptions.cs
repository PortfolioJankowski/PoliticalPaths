namespace PoliticalPaths.Infrastructure.Identity;

public sealed class SeedAdminOptions
{
    public const string SectionName = "Identity:SeedAdmin";

    public bool Enabled { get; set; } = true;
    public string? Email { get; set; }
    public string? Password { get; set; }
}
