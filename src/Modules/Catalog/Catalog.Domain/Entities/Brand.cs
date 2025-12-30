using TheShop.SharedKernel;

namespace Catalog.Domain.Entities;

public class Brand : EntityBase<Guid>
{
    public string Name { get; private set; } = string.Empty;

    private Brand() { } // EF Core

    public static Brand Create(string name)
    {
        return new Brand
        {
            Id = Guid.NewGuid(),
            Name = name
        };
    }

    public void UpdateName(string name)
    {
        Name = name;
    }
}

