namespace PoliticalPaths.Domain.Messaging;

public sealed class InboxMessage
{
    public Guid MessageId { get; set; }
    public string Consumer { get; set; } = null!;
    public DateTime ProcessedAtUtc { get; set; }
}
