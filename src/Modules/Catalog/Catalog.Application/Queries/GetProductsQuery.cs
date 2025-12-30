using Catalog.Application.Dtos;
using FluentResults;
using MediatR;
using TheShop.SharedKernel;

namespace Catalog.Application.Queries;

public record GetProductsQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    Guid? BrandId = null,
    Guid? CategoryId = null) : IRequest<Result<PagedResult<ProductDto>>>;

