using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Ordering.Domain.Aggregates;
using TheShop.Infrastructure.Data;

namespace TheShop.Infrastructure.Repositories;

public class OrderRepository : Repository<Order>
{
    public OrderRepository(TheShopDbContext context) : base(context)
    {
    }

    public override async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await DbSet
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id, ct);
    }

    public override async Task<IReadOnlyList<Order>> FindAsync(Expression<Func<Order, bool>> predicate, CancellationToken ct = default)
    {
        return await DbSet
            .Include(o => o.OrderItems)
            .Where(predicate)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(ct);
    }

    public override async Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken ct = default)
    {
        return await DbSet
            .Include(o => o.OrderItems)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(ct);
    }
}

