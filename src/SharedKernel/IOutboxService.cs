namespace TheShop.SharedKernel;

public interface IOutboxService
{
    Task SaveAsync(IIntegrationEvent @event, CancellationToken ct = default);
}

