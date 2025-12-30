using Basket.Domain.Entities;

namespace Basket.Application.Services;

public interface IBasketService
{
    Task<CustomerBasket?> GetBasketAsync(Guid buyerId, CancellationToken ct = default);
    Task<CustomerBasket> UpdateBasketAsync(CustomerBasket basket, CancellationToken ct = default);
    Task DeleteBasketAsync(Guid buyerId, CancellationToken ct = default);
    Task UpdatePriceInAllBasketsAsync(Guid productId, decimal newPrice, CancellationToken ct = default);
}

