using Catalog.Application.Dtos;
using Catalog.Domain.Entities;
using FluentResults;
using MediatR;
using TheShop.SharedKernel;

namespace Catalog.Application.Queries;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, Result<PagedResult<ProductDto>>>
{
    private readonly IRepository<Product> _productRepository;

    public GetProductsQueryHandler(IRepository<Product> productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<PagedResult<ProductDto>>> Handle(GetProductsQuery request, CancellationToken ct)
    {
        var allProducts = await _productRepository.GetAllAsync(ct);

        var filtered = allProducts.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            filtered = filtered.Where(p =>
                p.Name.Contains(request.Search, StringComparison.OrdinalIgnoreCase) ||
                (p.Description?.Contains(request.Search, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        if (request.BrandId.HasValue)
        {
            filtered = filtered.Where(p => p.BrandId == request.BrandId.Value);
        }

        if (request.CategoryId.HasValue)
        {
            filtered = filtered.Where(p => p.CategoryId == request.CategoryId.Value);
        }

        var totalCount = filtered.Count();
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var items = filtered
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProductDto(
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                p.ImageUrl,
                p.BrandId,
                p.Brand?.Name ?? string.Empty,
                p.CategoryId,
                p.Category?.Name ?? string.Empty,
                p.IsAvailable,
                p.CreatedAt))
            .ToList();

        return Result.Ok(new PagedResult<ProductDto>(items, request.Page, request.PageSize, totalCount, totalPages));
    }
}

