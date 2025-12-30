using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TheShop.ServiceDefaults;

public static class HealthCheckExtensions
{
     public static IServiceCollection AddInfrastructureHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var postgresConnection = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrEmpty(postgresConnection))
        {
            services.AddHealthChecks()
                .AddNpgSql(postgresConnection, name: "database", tags: ["ready"]);
        }

        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnection))
        {
            services.AddHealthChecks()
                .AddRedis(redisConnection, name: "redis", tags: ["ready"]);
        }

        return services;
    }
}

