using BookingService.Domain.Entities;
using BookingService.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingService.Infrastructure.Persistence.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.ToTable("bookings");

            builder.Property(b => b.Status)
                .HasConversion(
                    status => status.ToString().ToLowerInvariant(),
                    value => ParseBookingStatus(value));

            builder.Property(b => b.CreatedAt)
                .HasDefaultValueSql("NOW()");

            builder.HasIndex(b => new { b.PartnerCarId, b.StartTime, b.EndTime })
                .HasDatabaseName("idx_booking_car_time");

            builder.HasIndex(b => b.UserId)
                .HasDatabaseName("idx_booking_user");

            builder.HasIndex(b => b.PartnerId)
                .HasDatabaseName("idx_booking_partner");
        }

        private static BookingStatus ParseBookingStatus(string value)
        {
            return Enum.TryParse<BookingStatus>(value, true, out var parsed)
                ? parsed
                : BookingStatus.Pending;
        }
    }
}
