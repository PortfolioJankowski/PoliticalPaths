using System.ComponentModel.DataAnnotations;

namespace PoliticalPaths.Dashboard;

public sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    [Range(1, 10000)]
    public int AuthenticatedPermitLimit { get; set; } = 120;

    [Range(1, 10000)]
    public int AnonymousPermitLimit { get; set; } = 60;

    [Range(1, 1000)]
    public int LoginPermitLimit { get; set; } = 20;

    [Range(1, 1440)]
    public int LoginWindowMinutes { get; set; } = 5;

    public string[] KnownProxies { get; set; } = [];
}
