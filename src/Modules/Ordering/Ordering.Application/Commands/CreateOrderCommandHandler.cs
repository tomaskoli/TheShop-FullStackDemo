using Basket.Application.Services;
using FluentResults;
using Mapster;
using MediatR;
using Ordering.Application.Dtos;
using Ordering.Application.Mapping;
using Ordering.Domain.Aggregates;
using Ordering.Domain.Events;
using Ordering.Domain.ValueObjects;
using TheShop.SharedKernel;

namespace Ordering.Application.Commands;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<OrderDto>>
{
    private readonly IRepository<Order> _orderRepository;
    private readonly IBasketService _basketService;
    private readonly IOutboxService _outboxService;
    private readonly IProductPriceService _productPriceService;

    static CreateOrderCommandHandler()
    {
        OrderMappingConfig.Configure();
    }

    public CreateOrderCommandHandler(
        IRepository<Order> orderRepository,
        IBasketService basketService,
        IOutboxService outboxService,
        IProductPriceService productPriceService)
    {
        _orderRepository = orderRepository;
        _basketService = basketService;
        _outboxService = outboxService;
        _productPriceService = productPriceService;
    }

    public async Task<Result<OrderDto>> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        var basket = await _basketService.GetBasketAsync(request.BuyerId, ct);

        if (basket is null || basket.Items.Count == 0)
        {
            return Result.Fail<OrderDto>("Basket is empty");
        }

        var productIds = basket.Items.Select(i => i.ProductId).ToList();
        var productInfos = await _productPriceService.GetProductPriceInfoBatchAsync(productIds, ct);

        var missingProducts = productIds.Where(id => !productInfos.ContainsKey(id)).ToList();
        if (missingProducts.Count > 0)
        {
            return Result.Fail<OrderDto>($"Products not found: {string.Join(", ", missingProducts)}");
        }

        var unavailableProducts = productInfos.Values
            .Where(p => !p.IsAvailable)
            .Select(p => p.ProductName)
            .ToList();
        if (unavailableProducts.Count > 0)
        {
            return Result.Fail<OrderDto>($"Products not available: {string.Join(", ", unavailableProducts)}");
        }

        var address = new Address(
            request.ShippingStreet,
            request.ShippingCity,
            request.ShippingPostalCode,
            request.ShippingCountry);

        var orderItems = basket.Items.Select(basketItem =>
        {
            var productInfo = productInfos[basketItem.ProductId];
            return new OrderItemInput(
                productInfo.ProductId,
                productInfo.ProductName,
                productInfo.UnitPrice,
                basketItem.Quantity);
        });

        var order = Order.Create(request.BuyerId, address, orderItems);

        await _orderRepository.AddAsync(order, ct);

        // Save integration event in the same transaction as the order
        var integrationEvent = new OrderCreatedIntegrationEvent(
            order.Id,
            order.BuyerId,
            order.TotalAmount);
        await _outboxService.SaveAsync(integrationEvent, ct);

        await _orderRepository.SaveChangesAsync(ct);

        await _basketService.DeleteBasketAsync(request.BuyerId, ct);

        return Result.Ok(order.Adapt<OrderDto>());
    }
}
