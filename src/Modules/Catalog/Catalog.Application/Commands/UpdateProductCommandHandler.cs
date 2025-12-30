using Catalog.Application.Dtos;
using Catalog.Domain.Entities;
using Catalog.Domain.Events;
using FluentResults;
using MediatR;
using TheShop.SharedKernel;

namespace Catalog.Application.Commands;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result<ProductDto>>
{
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<Brand> _brandRepository;
    private readonly IRepository<Category> _categoryRepository;
    private readonly IOutboxService _outboxService;

    public UpdateProductCommandHandler(
        IRepository<Product> productRepository,
        IRepository<Brand> brandRepository,
        IRepository<Category> categoryRepository,
        IOutboxService outboxService)
    {
        _productRepository = productRepository;
        _brandRepository = brandRepository;
        _categoryRepository = categoryRepository;
        _outboxService = outboxService;
    }

    public async Task<Result<ProductDto>> Handle(UpdateProductCommand request, CancellationToken ct)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, ct);
        if (product is null)
        {
            return Result.Fail<ProductDto>("Product not found");
        }

        var brand = await _brandRepository.GetByIdAsync(request.BrandId, ct);
        if (brand is null)
        {
            return Result.Fail<ProductDto>("Brand not found");
        }

        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, ct);
        if (category is null)
        {
            return Result.Fail<ProductDto>("Category not found");
        }

        var oldPrice = product.Price;
        product.Update(
            request.Name,
            request.Description,
            request.Price,
            request.ImageUrl,
            request.BrandId,
            request.CategoryId);

        // If price changed, publish integration event
        if (oldPrice != request.Price)
        {
            var integrationEvent = new ProductPriceChangedIntegrationEvent(
                product.Id, oldPrice, request.Price);
            await _outboxService.SaveAsync(integrationEvent, ct);
        }

        await _productRepository.UpdateAsync(product, ct);
        await _productRepository.SaveChangesAsync(ct);

        return Result.Ok(new ProductDto(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.ImageUrl,
            product.BrandId,
            brand.Name,
            product.CategoryId,
            category.Name,
            product.IsAvailable,
            product.CreatedAt));
    }
}

