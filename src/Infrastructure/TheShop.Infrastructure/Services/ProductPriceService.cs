using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using TheShop.Infrastructure.Data;
using TheShop.SharedKernel;

namespace TheShop.Infrastructure.Services;

public class ProductPriceService : IProductPriceService
{
    private readonly TheShopDbContext _context;

    public ProductPriceService(TheShopDbContext context)
    {
        _context = context;
    }

    public async Task<ProductPriceInfo?> GetProductPriceInfoAsync(Guid productId, CancellationToken ct = default)
    {
        var product = await _context.Set<Product>()
            .AsNoTracking()
            .Where(p => p.Id == productId)
            .Select(p => new ProductPriceInfo(
                p.Id,
                p.Name,
                p.Price,
                p.ImageUrl,
                p.IsAvailable))
            .FirstOrDefaultAsync(ct);

        return product;
    }

    public async Task<IReadOnlyDictionary<Guid, ProductPriceInfo>> GetProductPriceInfoBatchAsync(
        IEnumerable<Guid> productIds, 
        CancellationToken ct = default)
    {
        var productIdList = productIds.ToList();
        
        if (productIdList.Count == 0)
        {
            return new Dictionary<Guid, ProductPriceInfo>();
        }

        var products = await _context.Set<Product>()
            .AsNoTracking()
            .Where(p => productIdList.Contains(p.Id))
            .Select(p => new ProductPriceInfo(
                p.Id,
                p.Name,
                p.Price,
                p.ImageUrl,
                p.IsAvailable))
            .ToDictionaryAsync(p => p.ProductId, ct);

        return products;
    }
}

