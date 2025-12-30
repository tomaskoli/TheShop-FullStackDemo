using FluentResults;
using MediatR;
using Ordering.Application.Dtos;
using TheShop.SharedKernel;

namespace Ordering.Application.Queries;

public record GetAllOrdersQuery(
    int Page = 1,
    int PageSize = 10) : IRequest<Result<PagedResult<OrderDto>>>;

