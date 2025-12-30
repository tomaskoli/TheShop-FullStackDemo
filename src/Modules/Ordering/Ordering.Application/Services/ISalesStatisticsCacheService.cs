using Ordering.Application.Dtos;

namespace Ordering.Application.Services;

public interface ISalesStatisticsCacheService
{
    Task<SalesStatisticsDto?> GetAsync(string cacheKey, CancellationToken ct = default);
    Task SetAsync(string cacheKey, SalesStatisticsDto statistics, TimeSpan expiry, CancellationToken ct = default);
    string BuildCacheKey(DateTime? fromDate, DateTime? toDate, Guid? categoryId, Guid? brandId);
}

