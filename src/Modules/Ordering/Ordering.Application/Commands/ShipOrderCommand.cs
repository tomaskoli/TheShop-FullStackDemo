using FluentResults;
using MediatR;

namespace Ordering.Application.Commands;

public record ShipOrderCommand(Guid OrderId) : IRequest<Result>;

