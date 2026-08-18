using Microsoft.EntityFrameworkCore;
using TheHive.Domain.Entities;

namespace TheHive.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<BrandAction> BrandActions { get; }
    DbSet<Checklist> Checklists { get; }
    DbSet<ChecklistItem> ChecklistItems { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<Agency> Agencies { get; }
    DbSet<Brand> Brands { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
