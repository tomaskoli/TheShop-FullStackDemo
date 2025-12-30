using System.Text.Json;
using Basket.Application.Services;
using Basket.Domain.Entities;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace TheShop.Infrastructure.Services;

public class BasketService : IBasketService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;
    private readonly ILogger<BasketService> _logger;
    private readonly TimeSpan _basketExpiration = TimeSpan.FromHours(24);

    public BasketService(IConnectionMultiplexer redis, ILogger<BasketService> logger)
    {
        _redis = redis;
        _database = redis.GetDatabase();
        _logger = logger;
    }

    public async Task<CustomerBasket?> GetBasketAsync(Guid buyerId, CancellationToken ct = default)
    {
        var key = GetBasketKey(buyerId);
        var data = await _database.StringGetAsync(key);

        if (data.IsNullOrEmpty)
        {
            return null;
        }

        return JsonSerializer.Deserialize<CustomerBasket>(data.ToString());
    }

    public async Task<CustomerBasket> UpdateBasketAsync(CustomerBasket basket, CancellationToken ct = default)
    {
        var key = GetBasketKey(basket.BuyerId);
        basket.LastUpdated = DateTime.UtcNow;

        var json = JsonSerializer.Serialize(basket);
        await _database.StringSetAsync(key, json, _basketExpiration);

        return basket;
    }

    public async Task DeleteBasketAsync(Guid buyerId, CancellationToken ct = default)
    {
        var key = GetBasketKey(buyerId);
        await _database.KeyDeleteAsync(key);
    }

    public async Task UpdatePriceInAllBasketsAsync(Guid productId, decimal newPrice, CancellationToken ct = default)
    {
        // In a real implementation, you'd track basket keys or use Redis SCAN
        // For simplicity, this is a no-op placeholder
        _logger.LogInformation(
            "Price update requested for product {ProductId} to {NewPrice}",
            productId, newPrice);
        await Task.CompletedTask;
    }

    private static string GetBasketKey(Guid buyerId) => $"basket:{buyerId}";
}

