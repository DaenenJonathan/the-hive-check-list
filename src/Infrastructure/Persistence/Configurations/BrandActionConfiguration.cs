using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheHive.Domain.Entities;

namespace TheHive.Infrastructure.Persistence.Configurations;

public class BrandActionConfiguration : IEntityTypeConfiguration<BrandAction>
{
    public void Configure(EntityTypeBuilder<BrandAction> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Name).IsRequired().HasMaxLength(200);
        builder.Property(a => a.Description).HasMaxLength(1000);
        builder.Property(a => a.Address).HasMaxLength(500);
        builder.Property(a => a.City).HasMaxLength(200);
        builder.Property(a => a.MaterialPhotoPath).HasMaxLength(500);
        builder.Property(a => a.ConsumablesPhotoPath).HasMaxLength(500);
        builder.HasOne(a => a.Brand).WithMany().HasForeignKey(a => a.BrandId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(a => a.Checklists).WithOne(c => c.BrandAction).HasForeignKey(c => c.BrandActionId);
    }
}
