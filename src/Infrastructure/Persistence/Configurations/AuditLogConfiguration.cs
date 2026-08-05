using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheHive.Domain.Entities;

namespace TheHive.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(l => l.Id);
        builder.Property(l => l.UserId).IsRequired().HasMaxLength(100);
        builder.Property(l => l.UserName).IsRequired().HasMaxLength(200);
        builder.Property(l => l.Action).IsRequired().HasMaxLength(100);
        builder.Property(l => l.EntityType).IsRequired().HasMaxLength(100);
        builder.Property(l => l.OldValue).HasMaxLength(2000);
        builder.Property(l => l.NewValue).HasMaxLength(2000);
        builder.HasIndex(l => l.EntityId);
        builder.HasIndex(l => l.OccurredAt);
    }
}
