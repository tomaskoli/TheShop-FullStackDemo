namespace TheShop.SharedKernel;

public interface IIdempotencyService
{
    Task<IdempotencyResult?> GetAsync(string key, CancellationToken ct = default);
    Task StoreAsync(string key, int statusCode, string responseBody, TimeSpan? expiry = null, CancellationToken ct = default);
}

public record IdempotencyResult(int StatusCode, string ResponseBody);

