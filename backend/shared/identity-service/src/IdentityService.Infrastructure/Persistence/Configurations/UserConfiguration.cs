using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(user => user.Id);
        builder.Property(user => user.Id).HasColumnName("id");
        builder.Property(user => user.Username).HasColumnName("username").HasMaxLength(100).IsRequired();
        builder.Property(user => user.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
        builder.Property(user => user.PasswordHash).HasColumnName("password_hash").HasMaxLength(255).IsRequired();

        builder.HasIndex(user => user.Username).IsUnique();
        builder.HasIndex(user => user.Email).IsUnique();

        builder.HasMany(user => user.Roles)
            .WithMany(role => role.Users)
            .UsingEntity<Dictionary<string, object>>(
                "user_roles",
                joinRole => joinRole
                    .HasOne<Role>()
                    .WithMany()
                    .HasForeignKey("role_id")
                    .OnDelete(DeleteBehavior.Cascade),
                joinUser => joinUser
                    .HasOne<User>()
                    .WithMany()
                    .HasForeignKey("user_id")
                    .OnDelete(DeleteBehavior.Cascade),
                joinBuilder =>
                {
                    joinBuilder.ToTable("user_roles");
                    joinBuilder.HasKey("user_id", "role_id");
                });
    }
}
