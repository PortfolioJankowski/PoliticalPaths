namespace PoliticalPaths.Domain.Messaging;

public sealed class OutboxMessage
{
    public Guid Id { get; set; }
    public string Type { get; set; } = null!;
    public string Payload { get; set; } = null!;
    public string Exchange { get; set; } = null!;
    public string RoutingKey { get; set; } = null!;
    public DateTime OccurredAtUtc { get; set; }
    public DateTime? ProcessedAtUtc { get; set; }
    public DateTime? NextAttemptAtUtc { get; set; }
    public int Attempts { get; set; }
    public string? LastError { get; set; }
}
