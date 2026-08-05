using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheHive.Domain.Entities;

namespace TheHive.Infrastructure.Persistence.Configurations;

public class ChecklistItemConfiguration : IEntityTypeConfiguration<ChecklistItem>
{
    public void Configure(EntityTypeBuilder<ChecklistItem> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.MaterialName).IsRequired().HasMaxLength(300);
        builder.Property(i => i.Location).HasMaxLength(200);
        builder.Property(i => i.Category).HasMaxLength(200);
        builder.Property(i => i.Notes).HasMaxLength(500);
        builder.Property(i => i.ImagePath).HasMaxLength(500);
        builder.Property(i => i.Remark).HasMaxLength(500);
        builder.HasIndex(i => i.ChecklistId);
    }
}
