using BookingService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingService.Infrastructure.Persistence.Configurations
{
    public sealed class PaymentSyncOutboxMessageConfiguration : IEntityTypeConfiguration<PaymentSyncOutboxMessage>
    {
        public void Configure(EntityTypeBuilder<PaymentSyncOutboxMessage> builder)
        {
            builder.ToTable("payment_sync_outbox_messages");

            builder.HasKey(message => message.Id);

            builder.Property(message => message.EventKey)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(message => message.EventType)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(message => message.Payload)
                .HasColumnType("jsonb")
                .IsRequired();

            builder.Property(message => message.AttemptCount)
                .HasDefaultValue(0);

            builder.Property(message => message.CreatedAt)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("NOW()");

            builder.Property(message => message.NextAttemptAt)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("NOW()");

            builder.Property(message => message.ProcessedAt)
                .HasColumnType("timestamp with time zone");

            builder.Property(message => message.LockedUntil)
                .HasColumnType("timestamp with time zone");

            builder.HasIndex(message => message.EventKey)
                .IsUnique();

            builder.HasIndex(message => new { message.ProcessedAt, message.NextAttemptAt, message.Id })
                .HasDatabaseName("idx_payment_sync_outbox_dispatch");

            builder.HasIndex(message => message.BookingId)
                .HasDatabaseName("idx_payment_sync_outbox_booking_id");
        }
    }
}
