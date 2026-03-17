using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Persistence.Configurations;

public sealed class UserProvisionRequestConfiguration : IEntityTypeConfiguration<UserProvisionRequest>
{
    public void Configure(EntityTypeBuilder<UserProvisionRequest> builder)
    {
        builder.ToTable("user_provision_requests");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Id).HasColumnName("id");
        builder.Property(item => item.RequestKey).HasColumnName("request_key").HasMaxLength(128).IsRequired();
        builder.Property(item => item.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(item => item.FullName).HasColumnName("full_name").HasMaxLength(300).IsRequired();
        builder.Property(item => item.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
        builder.Property(item => item.BirthDate).HasColumnName("birth_date").HasColumnType("date").IsRequired();
        builder.Property(item => item.SubjectType).HasColumnName("subject_type").HasMaxLength(64).IsRequired();
        builder.Property(item => item.ActorType).HasColumnName("actor_type").HasMaxLength(64).IsRequired();
        builder.Property(item => item.CreatedAtUtc).HasColumnName("created_at_utc").HasColumnType("timestamp with time zone").IsRequired();

        builder.HasIndex(item => item.RequestKey).IsUnique();
        builder.HasIndex(item => item.UserId);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(item => item.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
