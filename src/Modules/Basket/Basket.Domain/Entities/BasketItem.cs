namespace Basket.Domain.Entities;

public class BasketItem
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public string? ImageUrl { get; set; }

    public decimal TotalPrice => UnitPrice * Quantity;
}

