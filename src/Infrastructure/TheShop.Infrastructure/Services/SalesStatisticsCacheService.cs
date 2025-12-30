using System.Text.Json;
using Ordering.Application.Dtos;
using Ordering.Application.Services;
using StackExchange.Redis;

namespace TheShop.Infrastructure.Services;

public class SalesStatisticsCacheService : ISalesStatisticsCacheService
{
    private readonly IDatabase _database;
    private const string CachePrefix = "cache:sales-stats:";

    public SalesStatisticsCacheService(IConnectionMultiplexer redis)
    {
        _database = redis.GetDatabase();
    }

    public async Task<SalesStatisticsDto?> GetAsync(string cacheKey, CancellationToken ct = default)
    {
        var data = await _database.StringGetAsync(CachePrefix + cacheKey);
        if (data.IsNullOrEmpty)
        {
            return null;
        }

        return JsonSerializer.Deserialize<SalesStatisticsDto>(data.ToString());
    }

    public async Task SetAsync(string cacheKey, SalesStatisticsDto statistics, TimeSpan expiry, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(statistics);
        await _database.StringSetAsync(CachePrefix + cacheKey, json, expiry);
    }

    public string BuildCacheKey(DateTime? fromDate, DateTime? toDate, Guid? categoryId, Guid? brandId)
    {
        var from = fromDate?.ToString("yyyyMMdd") ?? "null";
        var to = toDate?.ToString("yyyyMMdd") ?? "null";
        var cat = categoryId?.ToString() ?? "null";
        var brand = brandId?.ToString() ?? "null";
        return $"{from}:{to}:{cat}:{brand}";
    }
}

