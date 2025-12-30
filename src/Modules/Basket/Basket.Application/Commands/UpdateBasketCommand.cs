using Basket.Application.Dtos;
using FluentResults;
using MediatR;

namespace Basket.Application.Commands;

public record UpdateBasketCommand(
    Guid BuyerId,
    Guid ProductId,
    int Quantity) : IRequest<Result<BasketDto>>;

