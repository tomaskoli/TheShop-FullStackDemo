using TheShop.SharedKernel;

namespace Catalog.Domain.Entities;

public class Category : EntityBase<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    private Category() { } // EF Core

    public static Category Create(string name, string? description = null)
    {
        return new Category
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description
        };
    }

    public void Update(string name, string? description)
    {
        Name = name;
        Description = description;
    }
}

