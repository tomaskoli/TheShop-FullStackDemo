using Basket.Application.Dtos;
using FluentResults;
using MediatR;

namespace Basket.Application.Queries;

public record GetBasketQuery(Guid BuyerId) : IRequest<Result<BasketDto>>;

