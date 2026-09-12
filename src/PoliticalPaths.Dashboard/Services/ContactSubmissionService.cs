using System.Text.Json;
using PoliticalPaths.Domain.Messaging;
using PoliticalPaths.Infrastructure.Persistence;
using PoliticalPaths.Shared.Messaging;

namespace PoliticalPaths.Dashboard.Services;

public sealed class ContactSubmissionService(AppDbContext db)
{
    public async Task SubmitAsync(Guid userId, string email, string category, string subject, string body, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var contact = new ContactMessage
        {
            Id = Guid.NewGuid(), UserId = userId, UserEmail = email,
            Category = category.Trim(), Subject = subject.Trim(), Body = body.Trim(), CreatedAtUtc = now
        };
        var message = new ContactMessageSubmitted(contact.Id, userId, email, contact.Category, contact.Subject, contact.Body, now);
        db.ContactMessages.Add(contact);
        db.OutboxMessages.Add(new OutboxMessage
        {
            Id = Guid.NewGuid(), Type = nameof(ContactMessageSubmitted), Payload = JsonSerializer.Serialize(message),
            Exchange = "politicalpaths.events", RoutingKey = "notification.discord.contact-submitted", OccurredAtUtc = now
        });
        await db.SaveChangesAsync(ct);
    }
}
