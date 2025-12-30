using FluentAssertions;
using Ordering.Domain.Aggregates;
using Ordering.Domain.Enums;
using Ordering.Domain.ValueObjects;
using Xunit;

namespace TheShop.UnitTests.Ordering;

public class OrderTests
{
    [Fact]
    public void Create_WithValidData_CreatesOrder()
    {
        // Arrange
        var buyerId = Guid.NewGuid();
        var address = new Address("Street", "City", "12345", "Country");
        var items = new[]
        {
            new OrderItemInput(Guid.NewGuid(), "Product 1", 10.00m, 2),
            new OrderItemInput(Guid.NewGuid(), "Product 2", 15.00m, 1)
        };

        // Act
        var order = Order.Create(buyerId, address, items);

        // Assert
        order.BuyerId.Should().Be(buyerId);
        order.Status.Should().Be(OrderStatus.Paid);
        order.OrderItems.Should().HaveCount(2);
        order.TotalAmount.Should().Be(35.00m);
    }

    [Fact]
    public void Cancel_WhenPaid_CancelsOrder()
    {
        // Arrange
        var order = CreateTestOrder();

        // Act
        var result = order.Cancel();

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenShipped_ReturnsFail()
    {
        // Arrange
        var order = CreateTestOrder();
        order.MarkAsShipped();

        // Act
        var result = order.Cancel();

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message.Contains("shipped"));
    }

    [Fact]
    public void MarkAsShipped_WhenPaid_ShipsOrder()
    {
        // Arrange
        var order = CreateTestOrder();

        // Act
        var result = order.MarkAsShipped();

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Shipped);
    }

    private static Order CreateTestOrder()
    {
        var buyerId = Guid.NewGuid();
        var address = new Address("Street", "City", "12345", "Country");
        var items = new[]
        {
            new OrderItemInput(Guid.NewGuid(), "Product", 10.00m, 1)
        };

        return Order.Create(buyerId, address, items);
    }
}

