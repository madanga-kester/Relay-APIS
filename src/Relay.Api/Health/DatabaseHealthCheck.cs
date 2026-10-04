using Microsoft.Extensions.Diagnostics.HealthChecks;
using Relay.Infrastructure.Persistence;

namespace Relay.Api.Health;

public sealed class DatabaseHealthCheck(RelayDbContext db) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try { return await db.Database.CanConnectAsync(cancellationToken) ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy("Database connection failed."); }
        catch (Exception exception) { return HealthCheckResult.Unhealthy("Database connection failed.", exception); }
    }
}
