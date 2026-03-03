using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Persistence.Configurations;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");

        builder.HasKey(role => role.Id);
        builder.Property(role => role.Id).HasColumnName("id");
        builder.Property(role => role.Name).HasColumnName("name").HasMaxLength(100).IsRequired();

        builder.HasIndex(role => role.Name).IsUnique();

        builder.HasMany(role => role.Permissions)
            .WithMany(permission => permission.Roles)
            .UsingEntity<Dictionary<string, object>>(
                "role_permissions",
                joinPermission => joinPermission
                    .HasOne<Permission>()
                    .WithMany()
                    .HasForeignKey("permission_id")
                    .OnDelete(DeleteBehavior.Cascade),
                joinRole => joinRole
                    .HasOne<Role>()
                    .WithMany()
                    .HasForeignKey("role_id")
                    .OnDelete(DeleteBehavior.Cascade),
                joinBuilder =>
                {
                    joinBuilder.ToTable("role_permissions");
                    joinBuilder.HasKey("role_id", "permission_id");
                });

        builder.HasMany(role => role.ParentRoles)
            .WithMany(role => role.ChildRoles)
            .UsingEntity<Dictionary<string, object>>(
                "role_inheritance",
                joinParentRole => joinParentRole
                    .HasOne<Role>()
                    .WithMany()
                    .HasForeignKey("parent_role_id")
                    .OnDelete(DeleteBehavior.Cascade),
                joinChildRole => joinChildRole
                    .HasOne<Role>()
                    .WithMany()
                    .HasForeignKey("child_role_id")
                    .OnDelete(DeleteBehavior.Cascade),
                joinBuilder =>
                {
                    joinBuilder.ToTable("role_inheritance");
                    joinBuilder.HasKey("child_role_id", "parent_role_id");
                });
    }
}
