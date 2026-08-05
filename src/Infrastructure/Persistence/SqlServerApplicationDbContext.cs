using Microsoft.EntityFrameworkCore;

namespace TheHive.Infrastructure.Persistence;

public class SqlServerApplicationDbContext : ApplicationDbContext
{
    public SqlServerApplicationDbContext(DbContextOptions<SqlServerApplicationDbContext> options) : base(options) { }
}
