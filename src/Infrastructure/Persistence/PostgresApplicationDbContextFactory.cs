using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TheHive.Infrastructure.Persistence;

/// <summary>
/// Used only by `dotnet ef migrations` at design time to generate PostgreSQL migrations
/// independently of the app's runtime configuration (no live database connection needed).
/// </summary>
public class PostgresApplicationDbContextFactory : IDesignTimeDbContextFactory<PostgresApplicationDbContext>
{
    public PostgresApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PostgresApplicationDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=TheHiveDb;Username=postgres;Password=postgres");
        return new PostgresApplicationDbContext(optionsBuilder.Options);
    }
}
