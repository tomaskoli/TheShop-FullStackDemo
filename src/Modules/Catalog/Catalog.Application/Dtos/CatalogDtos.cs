namespace Catalog.Application.Dtos;

public record ProductDto(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    string? ImageUrl,
    Guid BrandId,
    string BrandName,
    Guid CategoryId,
    string CategoryName,
    bool IsAvailable,
    DateTime CreatedAt);

public record CreateProductRequest(
    string Name,
    string? Description,
    decimal Price,
    string? ImageUrl,
    Guid BrandId,
    Guid CategoryId);

public record UpdateProductRequest(
    string Name,
    string? Description,
    decimal Price,
    string? ImageUrl,
    Guid BrandId,
    Guid CategoryId);

public record BrandDto(Guid Id, string Name);

public record CategoryDto(Guid Id, string Name, string? Description);

