using FluentResults;
using MediatR;
using Ordering.Application.Dtos;

namespace Ordering.Application.Queries;

public record GetOrderQuery(Guid OrderId) : IRequest<Result<OrderDto>>;

