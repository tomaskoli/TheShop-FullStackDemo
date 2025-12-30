namespace TheShop.SharedKernel;

public interface IProductPriceService
{
    Task<ProductPriceInfo?> GetProductPriceInfoAsync(Guid productId, CancellationToken ct = default);
    Task<IReadOnlyDictionary<Guid, ProductPriceInfo>> GetProductPriceInfoBatchAsync(
        IEnumerable<Guid> productIds, 
        CancellationToken ct = default);
}

public record ProductPriceInfo(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    string? ImageUrl,
    bool IsAvailable);

