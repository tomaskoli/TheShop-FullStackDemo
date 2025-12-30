using TheShop.SharedKernel;

namespace TheShop.Messaging.Abstractions;

public interface IEventBus
{
    Task PublishAsync<T>(T @event, CancellationToken ct = default) where T : IntegrationEvent;
}

