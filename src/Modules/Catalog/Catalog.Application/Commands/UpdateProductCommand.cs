using Catalog.Application.Dtos;
using FluentResults;
using MediatR;

namespace Catalog.Application.Commands;

public record UpdateProductCommand(
    Guid ProductId,
    string Name,
    string? Description,
    decimal Price,
    string? ImageUrl,
    Guid BrandId,
    Guid CategoryId) : IRequest<Result<ProductDto>>;

