using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace PoliticalPaths.Infrastructure.Messaging;

public sealed class RabbitMqTopologyProvisioner(
    RabbitMqConnection connection,
    IOptions<RabbitMqOptions> options,
    ILogger<RabbitMqTopologyProvisioner> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken ct)
    {
        if (!options.Value.Enabled) return;
        var broker = await connection.GetAsync(ct);
        await using var channel = await broker.CreateChannelAsync(cancellationToken: ct);
        foreach (var exchange in options.Value.Topology.Exchanges)
            await channel.ExchangeDeclareAsync(exchange.Name, exchange.Type, exchange.Durable, false, cancellationToken: ct);
        foreach (var queue in options.Value.Topology.Queues)
        {
            IDictionary<string, object?> arguments = new Dictionary<string, object?>();
            if (queue.DeadLetterExchange is { Length: > 0 }) arguments["x-dead-letter-exchange"] = queue.DeadLetterExchange;
            if (queue.Type is { Length: > 0 }) arguments["x-queue-type"] = queue.Type;
            if (queue.DeliveryLimit.HasValue) arguments["x-delivery-limit"] = queue.DeliveryLimit.Value;
            await channel.QueueDeclareAsync(queue.Name, queue.Durable, false, false, arguments, cancellationToken: ct);
        }
        foreach (var binding in options.Value.Topology.Bindings)
            await channel.QueueBindAsync(binding.Queue, binding.Exchange, binding.RoutingKey, cancellationToken: ct);
        logger.LogInformation("RabbitMQ topology confirmed: {ExchangeCount} exchanges, {QueueCount} queues, {BindingCount} bindings.",
            options.Value.Topology.Exchanges.Count, options.Value.Topology.Queues.Count, options.Value.Topology.Bindings.Count);
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
