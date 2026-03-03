using PartnerService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PartnerService.Infrastructure.Persistence.Configurations;

public class PartnerConfiguration : IEntityTypeConfiguration<Partner>
{
    public void Configure(EntityTypeBuilder<Partner> builder)
    {
        builder.ToTable("partners");

        builder.HasKey(partner => partner.Id);

        builder.Property(partner => partner.OwnerFirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(partner => partner.OwnerLastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(partner => partner.CreatedOn)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(partner => partner.ContractFileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(partner => partner.OwnerIdentityFileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(partner => partner.RegistrationDate)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(partner => partner.PartnershipEndDate)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(partner => partner.RelatedUserId)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(partner => partner.PhoneNumber)
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(partner => partner.RelatedUserId)
            .IsUnique();
    }
}
