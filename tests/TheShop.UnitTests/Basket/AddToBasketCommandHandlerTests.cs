using Basket.Application.Commands;
using Basket.Application.Services;
using Basket.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using TheShop.SharedKernel;
using Xunit;

namespace TheShop.UnitTests.Basket;

public class AddToBasketCommandHandlerTests
{
    private readonly IBasketService _basketService;
    private readonly IProductPriceService _productPriceService;
    private readonly AddToBasketCommandHandler _handler;

    public AddToBasketCommandHandlerTests()
    {
        _basketService = Substitute.For<IBasketService>();
        _productPriceService = Substitute.For<IProductPriceService>();
        _handler = new AddToBasketCommandHandler(_basketService, _productPriceService);
    }

    [Fact]
    public async Task Handle_WithValidProduct_AddsItemWithServerAuthoritativePrice()
    {
        // Arrange
        var buyerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var serverPrice = 99.99m;
        var serverName = "Server Product Name";
        var serverImageUrl = "https://example.com/image.jpg";

        _productPriceService.GetProductPriceInfoAsync(productId, Arg.Any<CancellationToken>())
            .Returns(new ProductPriceInfo(productId, serverName, serverPrice, serverImageUrl, true));

        _basketService.GetBasketAsync(buyerId, Arg.Any<CancellationToken>())
            .Returns((CustomerBasket?)null);

        _basketService.UpdateBasketAsync(Arg.Any<CustomerBasket>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var basket = callInfo.Arg<CustomerBasket>();
                return Task.FromResult(basket);
            });

        // Client only sends ProductId and Quantity - no price tampering possible
        var command = new AddToBasketCommand(buyerId, productId, 2);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new
            {
                ProductId = productId,
                ProductName = serverName, // Server-authoritative
                UnitPrice = serverPrice, // Server-authoritative
                Quantity = 2,
                ImageUrl = serverImageUrl // Server-authoritative
            });
    }

    [Fact]
    public async Task Handle_WithNonExistentProduct_ReturnsFail()
    {
        // Arrange
        var buyerId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        _productPriceService.GetProductPriceInfoAsync(productId, Arg.Any<CancellationToken>())
            .Returns((ProductPriceInfo?)null);

        var command = new AddToBasketCommand(buyerId, productId, 1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message.Contains("not found"));
    }

    [Fact]
    public async Task Handle_WithUnavailableProduct_ReturnsFail()
    {
        // Arrange
        var buyerId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        _productPriceService.GetProductPriceInfoAsync(productId, Arg.Any<CancellationToken>())
            .Returns(new ProductPriceInfo(productId, "Test Product", 10.00m, null, false)); // Not available

        var command = new AddToBasketCommand(buyerId, productId, 1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message.Contains("not available"));
    }

    [Fact]
    public async Task Handle_WithExistingBasket_AddsToExistingBasket()
    {
        // Arrange
        var buyerId = Guid.NewGuid();
        var existingProductId = Guid.NewGuid();
        var newProductId = Guid.NewGuid();

        var existingBasket = new CustomerBasket(buyerId)
        {
            Items =
            [
                new BasketItem
                {
                    ProductId = existingProductId,
                    ProductName = "Existing Product",
                    UnitPrice = 20.00m,
                    Quantity = 1
                }
            ]
        };

        _productPriceService.GetProductPriceInfoAsync(newProductId, Arg.Any<CancellationToken>())
            .Returns(new ProductPriceInfo(newProductId, "New Product", 30.00m, null, true));

        _basketService.GetBasketAsync(buyerId, Arg.Any<CancellationToken>())
            .Returns(existingBasket);

        _basketService.UpdateBasketAsync(Arg.Any<CustomerBasket>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(callInfo.Arg<CustomerBasket>()));

        var command = new AddToBasketCommand(buyerId, newProductId, 3);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
    }
}

