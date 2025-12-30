using Basket.Application.Dtos;
using Basket.Application.Services;
using Basket.Domain.Entities;
using FluentResults;
using MediatR;

namespace Basket.Application.Queries;

public class GetBasketQueryHandler : IRequestHandler<GetBasketQuery, Result<BasketDto>>
{
    private readonly IBasketService _basketService;

    public GetBasketQueryHandler(IBasketService basketService)
    {
        _basketService = basketService;
    }

    public async Task<Result<BasketDto>> Handle(GetBasketQuery request, CancellationToken ct)
    {
        var basket = await _basketService.GetBasketAsync(request.BuyerId, ct);

        if (basket is null)
        {
            basket = new CustomerBasket(request.BuyerId);
        }

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

