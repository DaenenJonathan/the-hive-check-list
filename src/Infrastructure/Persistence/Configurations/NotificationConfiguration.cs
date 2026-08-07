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
        builder.Property(n => n.ActionName).IsRequired().HasMaxLength(300);
        builder.Property(n => n.ChecklistName).IsRequired().HasMaxLength(300);
        builder.HasIndex(n => new { n.RecipientUserId, n.IsRead });
        builder.HasIndex(n => n.ChecklistId);
    }
}
