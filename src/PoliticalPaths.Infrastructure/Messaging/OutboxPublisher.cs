using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PoliticalPaths.Infrastructure.Persistence;
using RabbitMQ.Client;

namespace PoliticalPaths.Infrastructure.Messaging;

public sealed class OutboxPublisher(
    IServiceScopeFactory scopes,
    RabbitMqConnection connection,
    IOptions<RabbitMqOptions> options,
    ILogger<OutboxPublisher> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enabled) return;
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await PublishBatchAsync(stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
            catch (Exception ex) { logger.LogError(ex, "Outbox publishing cycle failed."); }
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private async Task PublishBatchAsync(CancellationToken ct)
    {
        await using var scope = scopes.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var now = DateTime.UtcNow;
        var messages = await db.OutboxMessages
            .Where(x => x.ProcessedAtUtc == null && (x.NextAttemptAtUtc == null || x.NextAttemptAtUtc <= now))
            .OrderBy(x => x.OccurredAtUtc).Take(25).ToListAsync(ct);
        if (messages.Count == 0) return;

        var broker = await connection.GetAsync(ct);
        await using var channel = await broker.CreateChannelAsync(
            new CreateChannelOptions(publisherConfirmationsEnabled: true, publisherConfirmationTrackingEnabled: true), ct);
        foreach (var message in messages)
        {
            try
            {
                var properties = new BasicProperties
                {
                    Persistent = true,
                    MessageId = message.Id.ToString(),
                    Type = message.Type,
                    ContentType = "application/json"
                };
                await channel.BasicPublishAsync(message.Exchange, message.RoutingKey, true, properties,
                    Encoding.UTF8.GetBytes(message.Payload), ct);
                message.ProcessedAtUtc = DateTime.UtcNow;
                message.LastError = null;
            }
            catch (Exception ex)
            {
                message.Attempts++;
                message.LastError = ex.Message.Length > 2000 ? ex.Message[..2000] : ex.Message;
                message.NextAttemptAtUtc = DateTime.UtcNow.AddSeconds(Math.Min(300, Math.Pow(2, message.Attempts)));
            }
        }
        await db.SaveChangesAsync(ct);
    }
}
