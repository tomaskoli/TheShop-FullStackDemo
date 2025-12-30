namespace Basket.Domain.Entities;

public class CustomerBasket
{
    public Guid BuyerId { get; set; }
    public List<BasketItem> Items { get; set; } = [];
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    public CustomerBasket() { }

    public CustomerBasket(Guid buyerId)
    {
        BuyerId = buyerId;
    }

    public decimal TotalPrice => Items.Sum(i => i.TotalPrice);

    public void AddItem(BasketItem item)
    {
        var existingItem = Items.FirstOrDefault(i => i.ProductId == item.ProductId);
        if (existingItem is not null)
        {
            existingItem.Quantity += item.Quantity;
        }
        else
        {
            Items.Add(item);
        }
        LastUpdated = DateTime.UtcNow;
    }

    public void UpdateItemQuantity(Guid productId, int quantity)
    {
        var item = Items.FirstOrDefault(i => i.ProductId == productId);
        if (item is not null)
        {
            if (quantity <= 0)
            {
                Items.Remove(item);
            }
            else
            {
                item.Quantity = quantity;
            }
            LastUpdated = DateTime.UtcNow;
        }
    }

    public void RemoveItem(Guid productId)
    {
        var item = Items.FirstOrDefault(i => i.ProductId == productId);
        if (item is not null)
        {
            Items.Remove(item);
            LastUpdated = DateTime.UtcNow;
        }
    }

    public void UpdatePrice(Guid productId, decimal newPrice)
    {
        var item = Items.FirstOrDefault(i => i.ProductId == productId);
        if (item is not null)
        {
            item.UnitPrice = newPrice;
            LastUpdated = DateTime.UtcNow;
        }
    }

    public void Clear()
    {
        Items.Clear();
        LastUpdated = DateTime.UtcNow;
    }
}

