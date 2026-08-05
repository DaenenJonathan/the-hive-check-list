using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheHive.Domain.Entities;

namespace TheHive.Infrastructure.Persistence.Configurations;

public class ChecklistConfiguration : IEntityTypeConfiguration<Checklist>
{
    public void Configure(EntityTypeBuilder<Checklist> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
        builder.Property(c => c.SourceFileName).HasMaxLength(500);
        builder.HasMany(c => c.Items).WithOne(i => i.Checklist).HasForeignKey(i => i.ChecklistId);
    }
}
