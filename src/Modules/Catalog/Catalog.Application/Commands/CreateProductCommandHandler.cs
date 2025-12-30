using Catalog.Application.Dtos;
using Catalog.Domain.Entities;
using FluentResults;
using MediatR;
using TheShop.SharedKernel;

namespace Catalog.Application.Commands;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<ProductDto>>
{
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<Brand> _brandRepository;
    private readonly IRepository<Category> _categoryRepository;

    public CreateProductCommandHandler(
        IRepository<Product> productRepository,
        IRepository<Brand> brandRepository,
        IRepository<Category> categoryRepository)
    {
        _productRepository = productRepository;
        _brandRepository = brandRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<ProductDto>> Handle(CreateProductCommand request, CancellationToken ct)
    {
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

        var product = Product.Create(
            request.Name,
            request.Description,
            request.Price,
            request.ImageUrl,
            request.BrandId,
            request.CategoryId);

        await _productRepository.AddAsync(product, ct);
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

