using FluentResults;
using MediatR;

namespace Ordering.Application.Commands;

public record CancelOrderCommand(Guid OrderId, Guid BuyerId) : IRequest<Result>;

