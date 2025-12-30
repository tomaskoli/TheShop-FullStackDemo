using Basket.Application.Dtos;
using Basket.Application.Services;
using Basket.Domain.Entities;
using FluentResults;
using MediatR;
using TheShop.SharedKernel;

namespace Basket.Application.Commands;

public class AddToBasketCommandHandler : IRequestHandler<AddToBasketCommand, Result<BasketDto>>
{
    private readonly IBasketService _basketService;
    private readonly IProductPriceService _productPriceService;

    public AddToBasketCommandHandler(
        IBasketService basketService,
        IProductPriceService productPriceService)
    {
        _basketService = basketService;
        _productPriceService = productPriceService;
    }

    public async Task<Result<BasketDto>> Handle(AddToBasketCommand request, CancellationToken ct)
    {
        // Resolve product information server-side to prevent price tampering
        var productInfo = await _productPriceService.GetProductPriceInfoAsync(request.ProductId, ct);
        
        if (productInfo is null)
        {
            return Result.Fail<BasketDto>("Product not found");
        }

        if (!productInfo.IsAvailable)
        {
            return Result.Fail<BasketDto>("Product is not available");
        }

        var basket = await _basketService.GetBasketAsync(request.BuyerId, ct)
                     ?? new CustomerBasket(request.BuyerId);

        var item = new BasketItem
        {
            ProductId = productInfo.ProductId,
            ProductName = productInfo.ProductName,
            UnitPrice = productInfo.UnitPrice,
            Quantity = request.Quantity,
            ImageUrl = productInfo.ImageUrl
        };

        basket.AddItem(item);

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
