using Catalog.Domain.Entities;
using FluentResults;
using MediatR;
using Ordering.Application.Dtos;
using Ordering.Application.Services;
using Ordering.Domain.Aggregates;
using Ordering.Domain.Enums;
using TheShop.SharedKernel;

namespace Ordering.Application.Queries;

public class GetSalesStatisticsQueryHandler : IRequestHandler<GetSalesStatisticsQuery, Result<SalesStatisticsDto>>
{
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<Brand> _brandRepository;
    private readonly IRepository<Category> _categoryRepository;
    private readonly ISalesStatisticsCacheService _cacheService;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(30);

    public GetSalesStatisticsQueryHandler(
        IRepository<Order> orderRepository,
        IRepository<Product> productRepository,
        IRepository<Brand> brandRepository,
        IRepository<Category> categoryRepository,
        ISalesStatisticsCacheService cacheService)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _brandRepository = brandRepository;
        _categoryRepository = categoryRepository;
        _cacheService = cacheService;
    }

    public async Task<Result<SalesStatisticsDto>> Handle(GetSalesStatisticsQuery request, CancellationToken ct)
    {
        var cacheKey = _cacheService.BuildCacheKey(request.FromDate, request.ToDate, request.CategoryId, request.BrandId);
        var cached = await _cacheService.GetAsync(cacheKey, ct);
        if (cached is not null)
        {
            return Result.Ok(cached);
        }

        var now = DateTime.UtcNow;
        var lastDayStart = now.AddDays(-1);
        var lastMonthStart = now.AddMonths(-1);

        var orders = await _orderRepository.FindAsync(
            o => o.Status != OrderStatus.Cancelled, ct);

        var products = await _productRepository.GetAllAsync(ct);
        var productLookup = products.ToDictionary(p => p.Id);

        var brands = await _brandRepository.GetAllAsync(ct);
        var brandNames = brands.ToDictionary(b => b.Id, b => b.Name);

        var categories = await _categoryRepository.GetAllAsync(ct);
        var categoryNames = categories.ToDictionary(c => c.Id, c => c.Name);

        var orderItems = orders
            .SelectMany(o => o.OrderItems.Select(i => new { Order = o, Item = i }))
            .Where(x => !request.FromDate.HasValue || x.Order.OrderDate >= request.FromDate.Value)
            .Where(x => !request.ToDate.HasValue || x.Order.OrderDate <= request.ToDate.Value)
            .ToList();

        if (request.CategoryId.HasValue)
        {
            orderItems = orderItems
                .Where(x => productLookup.TryGetValue(x.Item.ProductId, out var p) && p.CategoryId == request.CategoryId.Value)
                .ToList();
        }

        if (request.BrandId.HasValue)
        {
            orderItems = orderItems
                .Where(x => productLookup.TryGetValue(x.Item.ProductId, out var p) && p.BrandId == request.BrandId.Value)
                .ToList();
        }

        var lastDayItems = orderItems.Where(x => x.Order.OrderDate >= lastDayStart).ToList();
        var lastMonthItems = orderItems.Where(x => x.Order.OrderDate >= lastMonthStart).ToList();

        var salesByCategory = orderItems
            .Where(x => productLookup.ContainsKey(x.Item.ProductId))
            .GroupBy(x => productLookup[x.Item.ProductId].CategoryId)
            .Select(g =>
            {
                var categoryName = categoryNames.TryGetValue(g.Key, out var name)
                    ? name
                    : "Unknown";

                return new CategorySalesDto(
                    g.Key,
                    categoryName,
                    g.Sum(x => x.Item.Quantity),
                    g.Sum(x => x.Item.TotalPrice));
            })
            .OrderByDescending(x => x.QuantitySold)
            .ToList();

        var salesByBrand = orderItems
            .Where(x => productLookup.ContainsKey(x.Item.ProductId))
            .GroupBy(x => productLookup[x.Item.ProductId].BrandId)
            .Select(g =>
            {
                var brandName = brandNames.TryGetValue(g.Key, out var name)
                    ? name
                    : "Unknown";

                return new BrandSalesDto(
                    g.Key,
                    brandName,
                    g.Sum(x => x.Item.Quantity),
                    g.Sum(x => x.Item.TotalPrice));
            })
            .OrderByDescending(x => x.QuantitySold)
            .ToList();

        var statistics = new SalesStatisticsDto(
            lastDayItems.Sum(x => x.Item.Quantity),
            lastMonthItems.Sum(x => x.Item.Quantity),
            lastDayItems.Sum(x => x.Item.TotalPrice),
            lastMonthItems.Sum(x => x.Item.TotalPrice),
            salesByCategory,
            salesByBrand);

        await _cacheService.SetAsync(cacheKey, statistics, CacheDuration, ct);

        return Result.Ok(statistics);
    }
}

