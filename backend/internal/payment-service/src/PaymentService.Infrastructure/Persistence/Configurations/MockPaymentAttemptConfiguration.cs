using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;

namespace PaymentService.Infrastructure.Persistence.Configurations;

public sealed class MockPaymentAttemptConfiguration : IEntityTypeConfiguration<MockPaymentAttempt>
{
    public void Configure(EntityTypeBuilder<MockPaymentAttempt> builder)
    {
        builder.ToTable("mock_payment_attempts");

        builder.HasKey(attempt => attempt.Id);

        builder.Property(attempt => attempt.Amount)
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(attempt => attempt.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(attempt => attempt.SessionKey)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(attempt => attempt.Status)
            .HasConversion(
                value => value.ToString().ToLowerInvariant(),
                value => ParseStatus(value));

        builder.Property(attempt => attempt.CardHolder)
            .HasMaxLength(128);

        builder.Property(attempt => attempt.CardLast4)
            .HasMaxLength(4);

        builder.Property(attempt => attempt.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(attempt => attempt.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(attempt => attempt.CompletedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(attempt => attempt.ExpiresAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasIndex(attempt => attempt.SessionKey)
            .IsUnique();

        builder.HasIndex(attempt => new { attempt.BookingId, attempt.UserId, attempt.CreatedAt });
    }

    private static MockPaymentAttemptStatus ParseStatus(string value)
    {
        return Enum.TryParse<MockPaymentAttemptStatus>(value, true, out var parsed)
            ? parsed
            : MockPaymentAttemptStatus.Started;
    }
}
