using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(refreshToken => refreshToken.Id);
        builder.Property(refreshToken => refreshToken.Id).HasColumnName("id");
        builder.Property(refreshToken => refreshToken.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(refreshToken => refreshToken.TokenHash).HasColumnName("token_hash").HasMaxLength(128).IsRequired();
        builder.Property(refreshToken => refreshToken.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(refreshToken => refreshToken.ExpiresAtUtc).HasColumnName("expires_at_utc").IsRequired();
        builder.Property(refreshToken => refreshToken.RevokedAtUtc).HasColumnName("revoked_at_utc");

        builder.HasIndex(refreshToken => refreshToken.TokenHash).IsUnique();

        builder.HasOne(refreshToken => refreshToken.User)
            .WithMany(user => user.RefreshTokens)
            .HasForeignKey(refreshToken => refreshToken.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
