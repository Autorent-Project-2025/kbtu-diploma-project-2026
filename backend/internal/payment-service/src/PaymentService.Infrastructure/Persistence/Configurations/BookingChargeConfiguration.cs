using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;

namespace PaymentService.Infrastructure.Persistence.Configurations;

public sealed class BookingChargeConfiguration : IEntityTypeConfiguration<BookingCharge>
{
    public void Configure(EntityTypeBuilder<BookingCharge> builder)
    {
        builder.ToTable("booking_charges");

        builder.HasKey(charge => charge.Id);

        builder.Property(charge => charge.ChargeType)
            .HasConversion(
                value => value.ToString().ToLowerInvariant(),
                value => ParseChargeType(value));

        builder.Property(charge => charge.Status)
            .HasConversion(
                value => value.ToString().ToLowerInvariant(),
                value => ParseStatus(value));

        builder.Property(charge => charge.Amount)
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(charge => charge.PartnerShareAmount)
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(charge => charge.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(charge => charge.Description)
            .HasMaxLength(255);

        builder.Property(charge => charge.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(charge => charge.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(charge => charge.PaidAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(charge => charge.CanceledAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(charge => charge.RefundedAt)
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(charge => charge.BookingId);
        builder.HasIndex(charge => charge.UserId);
        builder.HasIndex(charge => charge.PartnerUserId);
        builder.HasIndex(charge => charge.Status);
    }

    private static BookingChargeType ParseChargeType(string value)
    {
        return Enum.TryParse<BookingChargeType>(value, true, out var parsed)
            ? parsed
            : BookingChargeType.LatePenalty;
    }

    private static BookingChargeStatus ParseStatus(string value)
    {
        return Enum.TryParse<BookingChargeStatus>(value, true, out var parsed)
            ? parsed
            : BookingChargeStatus.Pending;
    }
}
