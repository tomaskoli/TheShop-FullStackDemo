using System.Text.Json;
using StackExchange.Redis;
using TheShop.SharedKernel;

namespace TheShop.Infrastructure.Services;

public class IdempotencyService : IIdempotencyService
{
    private readonly IDatabase _redis;
    private const string KeyPrefix = "idempotency:";
    private static readonly TimeSpan DefaultExpiry = TimeSpan.FromHours(24);

    public IdempotencyService(IConnectionMultiplexer redis)
    {
        _redis = redis.GetDatabase();
    }

    public async Task<IdempotencyResult?> GetAsync(string key, CancellationToken ct = default)
    {
        var redisKey = KeyPrefix + key;
        var value = await _redis.StringGetAsync(redisKey);

        if (value.IsNullOrEmpty)
        {
            return null;
        }

        return JsonSerializer.Deserialize<IdempotencyResult>(value.ToString());
    }

    public async Task StoreAsync(string key, int statusCode, string responseBody, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        var redisKey = KeyPrefix + key;
        var result = new IdempotencyResult(statusCode, responseBody);
        var json = JsonSerializer.Serialize(result);

        await _redis.StringSetAsync(redisKey, json, expiry ?? DefaultExpiry);
    }
}

