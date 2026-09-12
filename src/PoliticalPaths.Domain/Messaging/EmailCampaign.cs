namespace PoliticalPaths.Domain.Messaging;

public sealed class EmailCampaign
{
    public Guid Id { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string Subject { get; set; } = null!;
    public string Body { get; set; } = null!;
    public DateTime CreatedAtUtc { get; set; }
    public int RecipientCount { get; set; }
    public ICollection<EmailDelivery> Deliveries { get; set; } = new List<EmailDelivery>();
}

public sealed class EmailDelivery
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public EmailCampaign Campaign { get; set; } = null!;
    public Guid UserId { get; set; }
    public string RecipientEmail { get; set; } = null!;
    public EmailDeliveryStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? SentAtUtc { get; set; }
    public string? ProviderMessageId { get; set; }
    public string? LastError { get; set; }
}

public enum EmailDeliveryStatus
{
    Pending,
    Sent,
    Failed,
    Skipped
}
