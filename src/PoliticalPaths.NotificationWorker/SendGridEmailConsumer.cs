using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PoliticalPaths.Domain.Messaging;
using PoliticalPaths.Infrastructure.Messaging;
using PoliticalPaths.Infrastructure.Persistence;
using PoliticalPaths.Shared.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PoliticalPaths.NotificationWorker;

public sealed class SendGridEmailConsumer(
    RabbitMqConnection connection,
    IServiceScopeFactory scopes,
    SendGridEmailSender sender,
    ILogger<SendGridEmailConsumer> logger) : BackgroundService
{
    private const string Queue = "notifications.email";
    private const string ConsumerName = "sendgrid-email-v1";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var broker = await connection.GetAsync(stoppingToken);
        await using var channel = await broker.CreateChannelAsync(cancellationToken: stoppingToken);
        await channel.BasicQosAsync(0, 1, false, stoppingToken);
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, delivery) =>
        {
            if (!Guid.TryParse(delivery.BasicProperties.MessageId, out var messageId))
            {
                await channel.BasicNackAsync(delivery.DeliveryTag, false, false, stoppingToken);
                return;
            }
            EmailNotificationRequested? message = null;
            try
            {
                message = JsonSerializer.Deserialize<EmailNotificationRequested>(Encoding.UTF8.GetString(delivery.Body.Span))
                    ?? throw new JsonException("Email payload is empty.");
                await using var scope = scopes.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                if (await db.InboxMessages.AnyAsync(x => x.MessageId == messageId && x.Consumer == ConsumerName, stoppingToken))
                {
                    await channel.BasicAckAsync(delivery.DeliveryTag, false, stoppingToken);
                    return;
                }
                var emailDelivery = await db.EmailDeliveries.SingleAsync(x => x.Id == message.DeliveryId, stoppingToken);
                if (emailDelivery.Status == EmailDeliveryStatus.Sent)
                {
                    db.InboxMessages.Add(new InboxMessage { MessageId = messageId, Consumer = ConsumerName, ProcessedAtUtc = DateTime.UtcNow });
                    await db.SaveChangesAsync(stoppingToken);
                    await channel.BasicAckAsync(delivery.DeliveryTag, false, stoppingToken);
                    return;
                }
                var recipientStillEligible = await db.Users.AsNoTracking().AnyAsync(x =>
                    x.Id == emailDelivery.UserId && x.IsActive && x.EmailConfirmed &&
                    x.EmailNotificationsEnabled && x.Email == emailDelivery.RecipientEmail, stoppingToken);
                if (!recipientStillEligible)
                {
                    emailDelivery.Status = EmailDeliveryStatus.Skipped;
                    emailDelivery.LastError = "Odbiorca wycofał zgodę, został zablokowany albo zmienił adres.";
                    db.InboxMessages.Add(new InboxMessage { MessageId = messageId, Consumer = ConsumerName, ProcessedAtUtc = DateTime.UtcNow });
                    await db.SaveChangesAsync(stoppingToken);
                    await channel.BasicAckAsync(delivery.DeliveryTag, false, stoppingToken);
                    return;
                }
                emailDelivery.ProviderMessageId = await sender.SendAsync(message, stoppingToken);
                emailDelivery.Status = EmailDeliveryStatus.Sent;
                emailDelivery.SentAtUtc = DateTime.UtcNow;
                emailDelivery.LastError = null;
                db.InboxMessages.Add(new InboxMessage { MessageId = messageId, Consumer = ConsumerName, ProcessedAtUtc = DateTime.UtcNow });
                await db.SaveChangesAsync(stoppingToken);
                await channel.BasicAckAsync(delivery.DeliveryTag, false, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "SendGrid notification {MessageId} failed.", messageId);
                if (message is not null) await MarkFailedAsync(message.DeliveryId, ex, stoppingToken);
                await channel.BasicNackAsync(delivery.DeliveryTag, false, true, stoppingToken);
            }
        };
        await channel.BasicConsumeAsync(Queue, false, ConsumerName, consumer, stoppingToken);
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task MarkFailedAsync(Guid deliveryId, Exception exception, CancellationToken ct)
    {
        try
        {
            await using var scope = scopes.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var delivery = await db.EmailDeliveries.SingleOrDefaultAsync(x => x.Id == deliveryId, ct);
            if (delivery is null || delivery.Status == EmailDeliveryStatus.Sent) return;
            delivery.Status = EmailDeliveryStatus.Failed;
            delivery.LastError = exception.Message.Length > 2000 ? exception.Message[..2000] : exception.Message;
            await db.SaveChangesAsync(ct);
        }
        catch (Exception persistenceError)
        {
            logger.LogError(persistenceError, "Could not persist failed e-mail delivery {DeliveryId}.", deliveryId);
        }
    }
}
