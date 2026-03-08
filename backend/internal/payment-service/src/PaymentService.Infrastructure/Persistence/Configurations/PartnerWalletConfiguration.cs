using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentService.Domain.Entities;

namespace PaymentService.Infrastructure.Persistence.Configurations;

public sealed class PartnerWalletConfiguration : IEntityTypeConfiguration<PartnerWallet>
{
    public void Configure(EntityTypeBuilder<PartnerWallet> builder)
    {
        builder.ToTable("partner_wallets");

        builder.HasKey(wallet => wallet.Id);

        builder.Property(wallet => wallet.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(wallet => wallet.PendingAmount)
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(wallet => wallet.AvailableAmount)
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(wallet => wallet.ReservedAmount)
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(wallet => wallet.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(wallet => wallet.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasIndex(wallet => wallet.PartnerUserId)
            .IsUnique();
    }
}
