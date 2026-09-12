namespace PoliticalPaths.Shared.Messaging;

public sealed record EmailNotificationRequested(
    Guid DeliveryId,
    Guid CampaignId,
    string RecipientEmail,
    string Subject,
    string Body,
    DateTime RequestedAtUtc);
