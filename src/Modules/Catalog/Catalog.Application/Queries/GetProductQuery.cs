using Catalog.Application.Dtos;
using FluentResults;
using MediatR;

namespace Catalog.Application.Queries;

public record GetProductQuery(Guid ProductId) : IRequest<Result<ProductDto>>;

