using FluentResults;
using Ordering.Domain.Entities;
using Ordering.Domain.Enums;
using Ordering.Domain.ValueObjects;
using TheShop.SharedKernel;

namespace Ordering.Domain.Aggregates;

public class Order : AggregateRoot<Guid>
{
    public Guid BuyerId { get; private set; }
    public DateTime OrderDate { get; private set; }
    public Address ShippingAddress { get; private set; } = null!;
    public OrderStatus Status { get; private set; }

    private readonly List<OrderItem> _orderItems = [];
    public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();

    public decimal TotalAmount => _orderItems.Sum(x => x.TotalPrice);

    private Order() { } // EF Core

    public static Order Create(Guid buyerId, Address address, IEnumerable<OrderItemInput> items)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            BuyerId = buyerId,
            OrderDate = DateTime.UtcNow,
            ShippingAddress = address,
            Status = OrderStatus.Paid // Simplified: skip payment processing
        };

        foreach (var item in items)
        {
            order.AddOrderItem(item.ProductId, item.ProductName, item.UnitPrice, item.Quantity);
        }

        return order;
    }

    public void AddOrderItem(Guid productId, string name, decimal price, int quantity)
    {
        var item = new OrderItem(productId, name, price, quantity);
        _orderItems.Add(item);
    }

    public Result Cancel()
    {
        if (Status == OrderStatus.Shipped)
        {
            return Result.Fail("Cannot cancel shipped order");
        }

        Status = OrderStatus.Cancelled;
        return Result.Ok();
    }

    public Result MarkAsShipped()
    {
        if (Status != OrderStatus.Paid)
        {
            return Result.Fail("Order must be paid before shipping");
        }

        Status = OrderStatus.Shipped;
        return Result.Ok();
    }
}

public record OrderItemInput(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity);

