using System.Text.Json;
using Identity.Application.Dtos;
using Identity.Application.Services;
using StackExchange.Redis;

namespace TheShop.Infrastructure.Services;

public class UserStatisticsCacheService : IUserStatisticsCacheService
{
    private readonly IDatabase _database;
    private const string CachePrefix = "cache:user-stats:";

    public UserStatisticsCacheService(IConnectionMultiplexer redis)
    {
        _database = redis.GetDatabase();
    }

    public async Task<UserStatisticsDto?> GetAsync(string cacheKey, CancellationToken ct = default)
    {
        var data = await _database.StringGetAsync(CachePrefix + cacheKey);
        if (data.IsNullOrEmpty)
        {
            return null;
        }

        return JsonSerializer.Deserialize<UserStatisticsDto>(data.ToString());
    }

    public async Task SetAsync(string cacheKey, UserStatisticsDto statistics, TimeSpan expiry, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(statistics);
        await _database.StringSetAsync(CachePrefix + cacheKey, json, expiry);
    }

    public string BuildCacheKey(DateTime? fromDate, DateTime? toDate)
    {
        var from = fromDate?.ToString("yyyyMMdd") ?? "null";
        var to = toDate?.ToString("yyyyMMdd") ?? "null";
        return $"{from}:{to}";
    }
}

