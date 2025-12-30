using Basket.Application.Dtos;
using FluentResults;
using MediatR;

namespace Basket.Application.Commands;

public record RemoveFromBasketCommand(Guid BuyerId, Guid ProductId) : IRequest<Result<BasketDto>>;

