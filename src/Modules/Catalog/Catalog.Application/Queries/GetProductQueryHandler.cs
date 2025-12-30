using Catalog.Application.Dtos;
using Catalog.Domain.Entities;
using FluentResults;
using MediatR;
using TheShop.SharedKernel;

namespace Catalog.Application.Queries;

public class GetProductQueryHandler : IRequestHandler<GetProductQuery, Result<ProductDto>>
{
    private readonly IRepository<Product> _productRepository;

    public GetProductQueryHandler(IRepository<Product> productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<ProductDto>> Handle(GetProductQuery request, CancellationToken ct)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, ct);

        if (product is null)
        {
            return Result.Fail<ProductDto>("Product not found");
        }

        return Result.Ok(new ProductDto(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.ImageUrl,
            product.BrandId,
            product.Brand?.Name ?? string.Empty,
            product.CategoryId,
            product.Category?.Name ?? string.Empty,
            product.IsAvailable,
            product.CreatedAt));
    }
}

