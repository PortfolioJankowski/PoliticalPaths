using System.ComponentModel.DataAnnotations;

namespace PoliticalPaths.Infrastructure.Messaging;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";
    public bool Enabled { get; set; }
    [Required] public string Host { get; set; } = "localhost";
    [Range(1, 65535)] public int Port { get; set; } = 5672;
    [Required] public string VirtualHost { get; set; } = "/";
    [Required] public string Username { get; set; } = "guest";
    [Required] public string Password { get; set; } = "guest";
    public bool UseTls { get; set; }
    public string ConnectionName { get; set; } = "politicalpaths";
    public RabbitMqTopologyOptions Topology { get; set; } = new();
}

public sealed class RabbitMqTopologyOptions
{
    public List<RabbitMqExchangeOptions> Exchanges { get; set; } = [];
    public List<RabbitMqQueueOptions> Queues { get; set; } = [];
    public List<RabbitMqBindingOptions> Bindings { get; set; } = [];
}

public sealed class RabbitMqExchangeOptions
{
    public string Name { get; set; } = null!;
    public string Type { get; set; } = "topic";
    public bool Durable { get; set; } = true;
}

public sealed class RabbitMqQueueOptions
{
    public string Name { get; set; } = null!;
    public bool Durable { get; set; } = true;
    public string? Type { get; set; }
    public int? DeliveryLimit { get; set; }
    public string? DeadLetterExchange { get; set; }
}

public sealed class RabbitMqBindingOptions
{
    public string Exchange { get; set; } = null!;
    public string Queue { get; set; } = null!;
    public string RoutingKey { get; set; } = null!;
}
