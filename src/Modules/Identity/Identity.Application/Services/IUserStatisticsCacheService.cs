using Identity.Application.Dtos;

namespace Identity.Application.Services;

public interface IUserStatisticsCacheService
{
    Task<UserStatisticsDto?> GetAsync(string cacheKey, CancellationToken ct = default);
    Task SetAsync(string cacheKey, UserStatisticsDto statistics, TimeSpan expiry, CancellationToken ct = default);
    string BuildCacheKey(DateTime? fromDate, DateTime? toDate);
}

