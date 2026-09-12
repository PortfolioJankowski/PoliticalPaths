using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PoliticalPaths.Infrastructure.Messaging;

public static class MessagingServiceCollectionExtensions
{
    public static IServiceCollection AddRabbitMqMessaging(this IServiceCollection services, IConfiguration configuration, bool publishOutbox)
    {
        services.AddOptions<RabbitMqOptions>().Bind(configuration.GetSection(RabbitMqOptions.SectionName)).ValidateDataAnnotations().ValidateOnStart();
        services.AddSingleton<RabbitMqConnection>();
        services.AddHostedService<RabbitMqTopologyProvisioner>();
        if (publishOutbox) services.AddHostedService<OutboxPublisher>();
        return services;
    }
}
