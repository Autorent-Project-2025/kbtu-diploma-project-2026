using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;

namespace PaymentService.Infrastructure.Persistence.Configurations;

public sealed class CustomerPaymentConfiguration : IEntityTypeConfiguration<CustomerPayment>
{
    public void Configure(EntityTypeBuilder<CustomerPayment> builder)
    {
        builder.ToTable("customer_payments");

        builder.HasKey(payment => payment.Id);

        builder.Property(payment => payment.PriceHour)
            .HasColumnType("numeric(18,2)");

        builder.Property(payment => payment.GrossAmount)
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(payment => payment.PlatformCommissionRate)
            .HasColumnType("numeric(5,4)")
            .IsRequired();

        builder.Property(payment => payment.PlatformCommissionAmount)
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(payment => payment.PartnerAmount)
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(payment => payment.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(payment => payment.Status)
            .HasConversion(
                value => value.ToString().ToLowerInvariant(),
                value => ParseStatus(value));

        builder.Property(payment => payment.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(payment => payment.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(payment => payment.ConfirmedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(payment => payment.AvailableAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(payment => payment.CanceledAt)
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(payment => payment.BookingId)
            .IsUnique();
    }

    private static CustomerPaymentStatus ParseStatus(string value)
    {
        return Enum.TryParse<CustomerPaymentStatus>(value, true, out var parsed)
            ? parsed
            : CustomerPaymentStatus.Pending;
    }
}
