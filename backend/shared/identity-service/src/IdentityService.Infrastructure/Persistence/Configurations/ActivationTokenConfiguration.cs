using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Persistence.Configurations;

public sealed class ActivationTokenConfiguration : IEntityTypeConfiguration<ActivationToken>
{
    public void Configure(EntityTypeBuilder<ActivationToken> builder)
    {
        builder.ToTable("activation_tokens");

        builder.HasKey(activationToken => activationToken.Id);
        builder.Property(activationToken => activationToken.Id).HasColumnName("id");
        builder.Property(activationToken => activationToken.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(activationToken => activationToken.TokenHash).HasColumnName("token_hash").HasMaxLength(128).IsRequired();
        builder.Property(activationToken => activationToken.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(activationToken => activationToken.ExpiresAtUtc).HasColumnName("expires_at_utc").IsRequired();
        builder.Property(activationToken => activationToken.UsedAtUtc).HasColumnName("used_at_utc");

        builder.HasIndex(activationToken => activationToken.TokenHash).IsUnique();
        builder.HasIndex(activationToken => activationToken.UserId);
        builder.HasIndex(activationToken => activationToken.ExpiresAtUtc);

        builder.HasOne(activationToken => activationToken.User)
            .WithMany()
            .HasForeignKey(activationToken => activationToken.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
