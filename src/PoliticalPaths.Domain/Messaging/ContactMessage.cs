namespace PoliticalPaths.Domain.Messaging;

public sealed class ContactMessage
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = null!;
    public string Category { get; set; } = null!;
    public string Subject { get; set; } = null!;
    public string Body { get; set; } = null!;
    public DateTime CreatedAtUtc { get; set; }
}
