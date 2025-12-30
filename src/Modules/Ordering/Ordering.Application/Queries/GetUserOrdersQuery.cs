using FluentResults;
using MediatR;
using Ordering.Application.Dtos;

namespace Ordering.Application.Queries;

public record GetUserOrdersQuery(Guid BuyerId) : IRequest<Result<IReadOnlyList<OrderDto>>>;

