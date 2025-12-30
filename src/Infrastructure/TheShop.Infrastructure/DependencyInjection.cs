using Basket.Application.Services;
using Catalog.Domain.Entities;
using Identity.Application.Services;
using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ordering.Application.Services;
using Ordering.Domain.Aggregates;
using StackExchange.Redis;
using TheShop.Infrastructure.Data;
using TheShop.Infrastructure.Repositories;
using TheShop.Infrastructure.Services;
using TheShop.Infrastructure.Settings;
using TheShop.SharedKernel;

namespace TheShop.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<TheShopDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<DbContext>(sp => sp.GetRequiredService<TheShopDbContext>());

        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")!));

        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IRepository<Product>, ProductRepository>();
        services.AddScoped<IRepository<Ordering.Domain.Aggregates.Order>, OrderRepository>();
        services.AddScoped<IRepository<ApplicationUser>, Repository<ApplicationUser>>();
        services.AddScoped<IRepository<Brand>, Repository<Brand>>();
        services.AddScoped<IRepository<Category>, Repository<Category>>();

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<IBasketService, BasketService>();
        services.AddScoped<IOutboxService, OutboxService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IProductPriceService, ProductPriceService>();
        services.AddScoped<IIdempotencyService, IdempotencyService>();
        services.AddScoped<ISalesStatisticsCacheService, SalesStatisticsCacheService>();
        services.AddScoped<IUserStatisticsCacheService, UserStatisticsCacheService>();

        services.AddHttpContextAccessor();

        return services;
    }
}

