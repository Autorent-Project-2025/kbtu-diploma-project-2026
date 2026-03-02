using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Persistence.Configurations;

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("permissions");

        builder.HasKey(permission => permission.Id);
        builder.Property(permission => permission.Id).HasColumnName("id");
        builder.Property(permission => permission.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
        builder.Property(permission => permission.Description).HasColumnName("description").HasMaxLength(500).IsRequired();

        builder.HasIndex(permission => permission.Name).IsUnique();
    }
}
