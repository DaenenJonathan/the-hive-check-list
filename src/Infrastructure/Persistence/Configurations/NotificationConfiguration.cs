using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheHive.Domain.Entities;

namespace TheHive.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(n => n.Id);
        builder.Property(n => n.RecipientUserId).IsRequired().HasMaxLength(450);
        builder.Property(n => n.ActionName).HasMaxLength(300);
        builder.Property(n => n.ChecklistName).HasMaxLength(300);
        builder.Property(n => n.RequesterName).HasMaxLength(200);
        builder.Property(n => n.RequesterEmail).HasMaxLength(256);
        builder.Property(n => n.Message).HasMaxLength(1000);
        builder.HasIndex(n => new { n.RecipientUserId, n.IsRead });
        builder.HasIndex(n => n.ChecklistId);
    }
}
