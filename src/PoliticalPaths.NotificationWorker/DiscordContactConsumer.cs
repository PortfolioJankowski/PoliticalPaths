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

public sealed class DiscordContactConsumer(
    RabbitMqConnection connection,
    IServiceScopeFactory scopes,
    DiscordWebhookSender sender,
    ILogger<DiscordContactConsumer> logger) : BackgroundService
{
    private const string Queue = "notifications.discord";
    private const string ConsumerName = "discord-contact-v1";

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
            try
            {
                await using var scope = scopes.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                if (await db.InboxMessages.AnyAsync(x => x.MessageId == messageId && x.Consumer == ConsumerName, stoppingToken))
                {
                    await channel.BasicAckAsync(delivery.DeliveryTag, false, stoppingToken);
                    return;
                }
                var message = JsonSerializer.Deserialize<ContactMessageSubmitted>(Encoding.UTF8.GetString(delivery.Body.Span))
                    ?? throw new JsonException("Contact message payload is empty.");
                await sender.SendAsync(message, stoppingToken);
                db.InboxMessages.Add(new InboxMessage { MessageId = messageId, Consumer = ConsumerName, ProcessedAtUtc = DateTime.UtcNow });
                await db.SaveChangesAsync(stoppingToken);
                await channel.BasicAckAsync(delivery.DeliveryTag, false, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Discord notification {MessageId} failed.", messageId);
                await channel.BasicNackAsync(delivery.DeliveryTag, false, true, stoppingToken);
            }
        };
        await channel.BasicConsumeAsync(Queue, false, ConsumerName, consumer, stoppingToken);
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
