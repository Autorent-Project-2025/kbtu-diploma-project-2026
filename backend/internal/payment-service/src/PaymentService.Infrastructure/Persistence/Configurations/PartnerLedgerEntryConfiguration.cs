using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;

namespace PaymentService.Infrastructure.Persistence.Configurations;

public sealed class PartnerLedgerEntryConfiguration : IEntityTypeConfiguration<PartnerLedgerEntry>
{
    public void Configure(EntityTypeBuilder<PartnerLedgerEntry> builder)
    {
        builder.ToTable("partner_ledger_entries");

        builder.HasKey(entry => entry.Id);

        builder.Property(entry => entry.EntryType)
            .HasConversion(
                value => value.ToString().ToLowerInvariant(),
                value => ParseEntryType(value));

        builder.Property(entry => entry.Bucket)
            .HasConversion(
                value => value.ToString().ToLowerInvariant(),
                value => ParseBucket(value));

        builder.Property(entry => entry.AmountDelta)
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(entry => entry.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(entry => entry.Description)
            .HasMaxLength(255);

        builder.Property(entry => entry.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasOne(entry => entry.PartnerWallet)
            .WithMany(wallet => wallet.LedgerEntries)
            .HasForeignKey(entry => entry.PartnerWalletId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(entry => entry.CustomerPayment)
            .WithMany(payment => payment.LedgerEntries)
            .HasForeignKey(entry => entry.CustomerPaymentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(entry => entry.PartnerPayout)
            .WithMany(payout => payout.LedgerEntries)
            .HasForeignKey(entry => entry.PartnerPayoutId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(entry => new { entry.PartnerWalletId, entry.CreatedAt });
        builder.HasIndex(entry => entry.BookingId);
    }

    private static LedgerEntryType ParseEntryType(string value)
    {
        return Enum.TryParse<LedgerEntryType>(value, true, out var parsed)
            ? parsed
            : LedgerEntryType.BookingPendingCredit;
    }

    private static LedgerBucket ParseBucket(string value)
    {
        return Enum.TryParse<LedgerBucket>(value, true, out var parsed)
            ? parsed
            : LedgerBucket.Pending;
    }
}
