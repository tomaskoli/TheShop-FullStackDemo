using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using TheShop.Infrastructure.Data;

namespace TheShop.Infrastructure.Repositories;

public class ProductRepository : Repository<Product>
{
    public ProductRepository(TheShopDbContext context) : base(context)
    {
    }

    public override async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await DbSet
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public override async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default)
    {
        return await DbSet
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .ToListAsync(ct);
    }
}

