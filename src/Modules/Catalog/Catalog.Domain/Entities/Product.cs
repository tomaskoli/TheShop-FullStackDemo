using TheShop.SharedKernel;

namespace Catalog.Domain.Entities;

public class Product : EntityBase<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public string? ImageUrl { get; private set; }
    public Guid BrandId { get; private set; }
    public Brand Brand { get; private set; } = null!;
    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; } = null!;
    public bool IsAvailable { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Product() { } // EF Core

    public static Product Create(
        string name,
        string? description,
        decimal price,
        string? imageUrl,
        Guid brandId,
        Guid categoryId)
    {
        return new Product
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Price = price,
            ImageUrl = imageUrl,
            BrandId = brandId,
            CategoryId = categoryId,
            IsAvailable = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        string name,
        string? description,
        decimal price,
        string? imageUrl,
        Guid brandId,
        Guid categoryId)
    {
        Name = name;
        Description = description;
        Price = price;
        ImageUrl = imageUrl;
        BrandId = brandId;
        CategoryId = categoryId;
    }

    public decimal UpdatePrice(decimal newPrice)
    {
        var oldPrice = Price;
        Price = newPrice;
        return oldPrice;
    }

    public void SetAvailability(bool isAvailable)
    {
        IsAvailable = isAvailable;
    }
}

