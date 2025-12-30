using TheShop.SharedKernel;

namespace Ordering.Domain.Events;

public record OrderCreatedIntegrationEvent(
    Guid OrderId,
    Guid BuyerId,
    decimal TotalAmount) : IntegrationEvent;

public record OrderStatusChangedIntegrationEvent(
    Guid OrderId,
    string OldStatus,
    string NewStatus) : IntegrationEvent;

