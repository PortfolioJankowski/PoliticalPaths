namespace PoliticalPaths.Shared.Messaging;

public sealed record ContactMessageSubmitted(
    Guid MessageId,
    Guid UserId,
    string UserEmail,
    string Category,
    string Subject,
    string Body,
    DateTime SubmittedAtUtc);
