using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Relay.Infrastructure.Persistence;

public sealed class RelayDbContextFactory : IDesignTimeDbContextFactory<RelayDbContext>
{
    public RelayDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<RelayDbContext>();
        builder.UseNpgsql(Environment.GetEnvironmentVariable("ConnectionStrings__Postgres") ?? "Host=localhost;Port=5432;Database=relay;Username=relay_app;Password=relay_dev_only");
        return new RelayDbContext(builder.Options);
    }
}
