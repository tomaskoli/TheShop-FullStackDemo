using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheShop.Messaging.Abstractions;
using TheShop.Messaging.Kafka;
using TheShop.Messaging.Outbox;

namespace TheShop.Messaging;

public static class DependencyInjection
{
    public static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<KafkaSettings>()
            .Bind(configuration.GetSection("Kafka"));

        services.AddSingleton<IEventBus, KafkaEventBus>();
        services.AddHostedService<OutboxProcessor>();

        return services;
    }
}

