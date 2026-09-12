using System.ComponentModel.DataAnnotations;

namespace PoliticalPaths.NotificationWorker;

public sealed class SendGridOptions
{
    public const string SectionName = "SendGrid";
    public bool Enabled { get; set; }
    [Required] public string ApiKey { get; set; } = null!;
    [Required, EmailAddress] public string FromEmail { get; set; } = null!;
    [Required] public string FromName { get; set; } = "Political Paths";
}
