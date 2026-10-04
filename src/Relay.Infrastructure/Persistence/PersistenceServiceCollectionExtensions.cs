using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Relay.Infrastructure.Persistence;

public static class PersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddRelayPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connection = configuration.GetConnectionString("Postgres") ?? configuration["ConnectionStrings:Postgres"];
        if (string.IsNullOrWhiteSpace(connection))
        {
            services.AddDbContextPool<RelayDbContext>(_ => { });
            return services;
        }

        services.AddDbContextPool<RelayDbContext>(options =>
        {
            options.UseNpgsql(connection, npgsql => npgsql.EnableRetryOnFailure(3));
            options.EnableDetailedErrors(false);
            options.EnableSensitiveDataLogging(false);
        });
        return services;
    }
}
