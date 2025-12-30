using TheShop.SharedKernel;

namespace Catalog.Domain.Events;

public record ProductPriceChangedIntegrationEvent(
    Guid ProductId,
    decimal OldPrice,
    decimal NewPrice) : IntegrationEvent;

