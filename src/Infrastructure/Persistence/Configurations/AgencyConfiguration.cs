using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheHive.Domain.Entities;

namespace TheHive.Infrastructure.Persistence.Configurations;

public class AgencyConfiguration : IEntityTypeConfiguration<Agency>
{
    public void Configure(EntityTypeBuilder<Agency> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Name).IsRequired().HasMaxLength(200);
        builder.Property(a => a.Color).IsRequired().HasMaxLength(7);
        builder.HasIndex(a => a.Name).IsUnique();
        builder.HasMany(a => a.Brands).WithOne(b => b.Agency).HasForeignKey(b => b.AgencyId).OnDelete(DeleteBehavior.Restrict);
    }
}
