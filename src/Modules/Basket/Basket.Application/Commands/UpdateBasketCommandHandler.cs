using Basket.Application.Dtos;
using Basket.Application.Services;
using Basket.Domain.Entities;
using FluentResults;
using MediatR;

namespace Basket.Application.Commands;

public class UpdateBasketCommandHandler : IRequestHandler<UpdateBasketCommand, Result<BasketDto>>
{
    private readonly IBasketService _basketService;

    public UpdateBasketCommandHandler(IBasketService basketService)
    {
        _basketService = basketService;
    }

    public async Task<Result<BasketDto>> Handle(UpdateBasketCommand request, CancellationToken ct)
    {
        var basket = await _basketService.GetBasketAsync(request.BuyerId, ct);

        if (basket is null)
        {
            return Result.Fail<BasketDto>("Basket not found");
        }

        basket.UpdateItemQuantity(request.ProductId, request.Quantity);

        basket = await _basketService.UpdateBasketAsync(basket, ct);

        return Result.Ok(MapToDto(basket));
    }

    private static BasketDto MapToDto(CustomerBasket basket)
    {
        return new BasketDto(
            basket.BuyerId,
            basket.Items.Select(i => new BasketItemDto(
                i.ProductId,
                i.ProductName,
                i.UnitPrice,
                i.Quantity,
                i.ImageUrl,
                i.TotalPrice)).ToList(),
            basket.TotalPrice,
            basket.LastUpdated);
    }
}

