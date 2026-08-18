using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheHive.Domain.Entities;
using TheHive.Infrastructure.Identity;

namespace TheHive.Infrastructure.Persistence.Configurations;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.HasOne<Agency>().WithMany().HasForeignKey(u => u.AgencyId).OnDelete(DeleteBehavior.SetNull);
        builder.HasMany(u => u.ManagedBrands).WithMany().UsingEntity(j => j.ToTable("UserManagedBrands"));
    }
}
