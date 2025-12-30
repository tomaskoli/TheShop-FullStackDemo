using TheShop.SharedKernel;

namespace TheShop.Messaging.Abstractions;

public interface IIntegrationEventHandler<in T> where T : IntegrationEvent
{
    Task HandleAsync(T @event, CancellationToken ct = default);
}

