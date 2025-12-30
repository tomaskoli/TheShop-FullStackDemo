using TheShop.SharedKernel;

namespace Ordering.Domain.Entities;

public class OrderItem : EntityBase<Guid>
{
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public Guid OrderId { get; private set; }

    public decimal TotalPrice => UnitPrice * Quantity;

    private OrderItem() { } // EF Core

    public OrderItem(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }
}

