using Basket.Application.Services;
using Basket.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using Ordering.Application.Commands;
using Ordering.Domain.Aggregates;
using TheShop.SharedKernel;
using Xunit;

namespace TheShop.UnitTests.Ordering;

public class CreateOrderCommandHandlerTests
{
    private readonly IRepository<Order> _orderRepository;
    private readonly IBasketService _basketService;
    private readonly IOutboxService _outboxService;
    private readonly IProductPriceService _productPriceService;
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        _orderRepository = Substitute.For<IRepository<Order>>();
        _basketService = Substitute.For<IBasketService>();
        _outboxService = Substitute.For<IOutboxService>();
        _productPriceService = Substitute.For<IProductPriceService>();
        _handler = new CreateOrderCommandHandler(
            _orderRepository, 
            _basketService, 
            _outboxService,
            _productPriceService);
    }

    [Fact]
    public async Task Handle_WithValidBasket_CreatesOrderWithServerAuthoritativePrices()
    {
        // Arrange
        var buyerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var basket = new CustomerBasket(buyerId)
        {
            Items =
            [
                new BasketItem
                {
                    ProductId = productId,
                    ProductName = "Tampered Name", // Client might have tampered
                    UnitPrice = 0.01m, // Tampered price - should be ignored!
                    Quantity = 2
                }
            ]
        };

        _basketService.GetBasketAsync(buyerId, Arg.Any<CancellationToken>())
            .Returns(basket);

        // Server-authoritative product info - this is the source of truth
        var serverPrice = 10.00m;
        _productPriceService.GetProductPriceInfoBatchAsync(
            Arg.Any<IEnumerable<Guid>>(), 
            Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, ProductPriceInfo>
            {
                { productId, new ProductPriceInfo(productId, "Test Product", serverPrice, null, true) }
            });

        var command = new CreateOrderCommand(
            buyerId, "123 Street", "City", "12345", "Country");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.BuyerId.Should().Be(buyerId);
        // Price should be server-authoritative, not the tampered 0.01
        result.Value.TotalAmount.Should().Be(serverPrice * 2); // 10.00 * 2 = 20.00
        result.Value.Items.Should().ContainSingle()
            .Which.ProductName.Should().Be("Test Product"); // Server name, not tampered
        await _orderRepository.Received(1).AddAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
        await _basketService.Received(1).DeleteBasketAsync(buyerId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithEmptyBasket_ReturnsFail()
    {
        // Arrange
        var buyerId = Guid.NewGuid();
        _basketService.GetBasketAsync(buyerId, Arg.Any<CancellationToken>())
            .Returns(new CustomerBasket(buyerId) { Items = [] });

        var command = new CreateOrderCommand(
            buyerId, "123 Street", "City", "12345", "Country");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message.Contains("empty"));
    }

    [Fact]
    public async Task Handle_WithNullBasket_ReturnsFail()
    {
        // Arrange
        var buyerId = Guid.NewGuid();
        _basketService.GetBasketAsync(buyerId, Arg.Any<CancellationToken>())
            .Returns((CustomerBasket?)null);

        var command = new CreateOrderCommand(
            buyerId, "123 Street", "City", "12345", "Country");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithUnavailableProduct_ReturnsFail()
    {
        // Arrange
        var buyerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var basket = new CustomerBasket(buyerId)
        {
            Items =
            [
                new BasketItem
                {
                    ProductId = productId,
                    ProductName = "Test Product",
                    UnitPrice = 10.00m,
                    Quantity = 1
                }
            ]
        };

        _basketService.GetBasketAsync(buyerId, Arg.Any<CancellationToken>())
            .Returns(basket);

        _productPriceService.GetProductPriceInfoBatchAsync(
            Arg.Any<IEnumerable<Guid>>(), 
            Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, ProductPriceInfo>
            {
                { productId, new ProductPriceInfo(productId, "Test Product", 10.00m, null, false) } // Not available
            });

        var command = new CreateOrderCommand(
            buyerId, "123 Street", "City", "12345", "Country");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message.Contains("not available"));
    }

    [Fact]
    public async Task Handle_WithProductNotFound_ReturnsFail()
    {
        // Arrange
        var buyerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var basket = new CustomerBasket(buyerId)
        {
            Items =
            [
                new BasketItem
                {
                    ProductId = productId,
                    ProductName = "Test Product",
                    UnitPrice = 10.00m,
                    Quantity = 1
                }
            ]
        };

        _basketService.GetBasketAsync(buyerId, Arg.Any<CancellationToken>())
            .Returns(basket);

        // Product doesn't exist in catalog
        _productPriceService.GetProductPriceInfoBatchAsync(
            Arg.Any<IEnumerable<Guid>>(), 
            Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, ProductPriceInfo>()); // Empty - product not found

        var command = new CreateOrderCommand(
            buyerId, "123 Street", "City", "12345", "Country");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message.Contains("not found"));
    }
}
