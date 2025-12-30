using Catalog.Domain.Entities;
using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Ordering.Domain.Aggregates;
using Ordering.Domain.Entities;
using TheShop.Infrastructure.Data.Configurations;
using TheShop.SharedKernel;

namespace TheShop.Infrastructure.Data;

public class TheShopDbContext : DbContext
{
    public TheShopDbContext(DbContextOptions<TheShopDbContext> options) : base(options)
    {
    }

    // Identity
    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();

    // Catalog
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Category> Categories => Set<Category>();

    // Ordering
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    // Outbox
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Ensure schemas exist
        modelBuilder.HasDefaultSchema("public");
        
        // Apply schema separation
        modelBuilder.Entity<ApplicationUser>().ToTable("Users", "identity");
        modelBuilder.Entity<Product>().ToTable("Products", "catalog");
        modelBuilder.Entity<Brand>().ToTable("Brands", "catalog");
        modelBuilder.Entity<Category>().ToTable("Categories", "catalog");
        modelBuilder.Entity<Order>().ToTable("Orders", "ordering");
        modelBuilder.Entity<OrderItem>().ToTable("OrderItems", "ordering");
        modelBuilder.Entity<OutboxMessage>().ToTable("OutboxMessages", "outbox");

        // Apply configurations
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new BrandConfiguration());
        modelBuilder.ApplyConfiguration(new CategoryConfiguration());
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new OrderItemConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
    }
}

