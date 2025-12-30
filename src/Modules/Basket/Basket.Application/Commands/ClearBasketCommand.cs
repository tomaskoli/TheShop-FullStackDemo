using FluentResults;
using MediatR;

namespace Basket.Application.Commands;

public record ClearBasketCommand(Guid BuyerId) : IRequest<Result>;

