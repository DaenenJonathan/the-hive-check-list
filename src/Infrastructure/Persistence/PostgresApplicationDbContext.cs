using Microsoft.EntityFrameworkCore;

namespace TheHive.Infrastructure.Persistence;

public class PostgresApplicationDbContext : ApplicationDbContext
{
    public PostgresApplicationDbContext(DbContextOptions<PostgresApplicationDbContext> options) : base(options) { }
}
