using System.ComponentModel.DataAnnotations;

namespace PoliticalPaths.NotificationWorker;

public sealed class DiscordOptions
{
    public const string SectionName = "Discord";
    [Required, Url] public string WebhookUrl { get; set; } = null!;
    public string Username { get; set; } = "Political Paths";
}
