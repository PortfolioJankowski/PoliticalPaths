using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PoliticalPaths.Domain.Messaging;
using PoliticalPaths.Infrastructure.Persistence;
using PoliticalPaths.Shared.Messaging;

namespace PoliticalPaths.Dashboard.Services;

public sealed record EmailCampaignRow(Guid Id, string Subject, DateTime CreatedAtUtc, int Recipients, int Sent, int Failed, int Skipped);

public sealed class EmailCampaignService(AppDbContext db)
{
    public Task<int> GetSubscriberCountAsync(CancellationToken ct = default) => db.Users.AsNoTracking()
        .CountAsync(x => x.IsActive && x.EmailConfirmed && x.EmailNotificationsEnabled && x.Email != null, ct);

    public async Task<Guid> CreateAsync(Guid administratorId, string subject, string body, CancellationToken ct = default)
    {
        var recipients = await db.Users.AsNoTracking()
            .Where(x => x.IsActive && x.EmailConfirmed && x.EmailNotificationsEnabled && x.Email != null)
            .Select(x => new { x.Id, Email = x.Email! }).ToListAsync(ct);
        var now = DateTime.UtcNow;
        var campaign = new EmailCampaign
        {
            Id = Guid.NewGuid(), CreatedByUserId = administratorId, Subject = subject.Trim(), Body = body.Trim(),
            CreatedAtUtc = now, RecipientCount = recipients.Count
        };
        db.EmailCampaigns.Add(campaign);
        foreach (var recipient in recipients)
        {
            var delivery = new EmailDelivery
            {
                Id = Guid.NewGuid(), CampaignId = campaign.Id, UserId = recipient.Id,
                RecipientEmail = recipient.Email, Status = EmailDeliveryStatus.Pending, CreatedAtUtc = now
            };
            db.EmailDeliveries.Add(delivery);
            var message = new EmailNotificationRequested(delivery.Id, campaign.Id, recipient.Email, campaign.Subject, campaign.Body, now);
            db.OutboxMessages.Add(new OutboxMessage
            {
                Id = Guid.NewGuid(), Type = nameof(EmailNotificationRequested), Payload = JsonSerializer.Serialize(message),
                Exchange = "politicalpaths.events", RoutingKey = "notification.email.broadcast", OccurredAtUtc = now
            });
        }
        await db.SaveChangesAsync(ct);
        return campaign.Id;
    }

    public async Task<List<EmailCampaignRow>> GetRecentAsync(CancellationToken ct = default) =>
        await db.EmailCampaigns.AsNoTracking().OrderByDescending(x => x.CreatedAtUtc).Take(20)
            .Select(x => new EmailCampaignRow(x.Id, x.Subject, x.CreatedAtUtc, x.RecipientCount,
                x.Deliveries.Count(d => d.Status == EmailDeliveryStatus.Sent),
                x.Deliveries.Count(d => d.Status == EmailDeliveryStatus.Failed),
                x.Deliveries.Count(d => d.Status == EmailDeliveryStatus.Skipped)))
            .ToListAsync(ct);
}
