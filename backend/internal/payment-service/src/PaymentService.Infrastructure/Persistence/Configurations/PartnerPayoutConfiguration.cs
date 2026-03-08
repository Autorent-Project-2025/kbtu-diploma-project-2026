using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;

namespace PaymentService.Infrastructure.Persistence.Configurations;

public sealed class PartnerPayoutConfiguration : IEntityTypeConfiguration<PartnerPayout>
{
    public void Configure(EntityTypeBuilder<PartnerPayout> builder)
    {
        builder.ToTable("partner_payouts");

        builder.HasKey(payout => payout.Id);

        builder.Property(payout => payout.Amount)
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(payout => payout.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(payout => payout.Status)
            .HasConversion(
                value => value.ToString().ToLowerInvariant(),
                value => ParseStatus(value));

        builder.Property(payout => payout.RequestedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(payout => payout.ProcessedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(payout => payout.FailureReason)
            .HasMaxLength(2048);
    }

    private static PartnerPayoutStatus ParseStatus(string value)
    {
        return Enum.TryParse<PartnerPayoutStatus>(value, true, out var parsed)
            ? parsed
            : PartnerPayoutStatus.Requested;
    }
}
