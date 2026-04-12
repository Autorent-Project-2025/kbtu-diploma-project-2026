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

            builder.Property(b => b.PricingBreakdownJson)
                .HasColumnType("jsonb");

            builder.Property(b => b.ImageUrlsJson)
                .HasColumnType("jsonb");

            builder.Property(b => b.CarBrand)
                .HasMaxLength(255);

            builder.Property(b => b.CarModel)
                .HasMaxLength(255);

            builder.Property(b => b.PartnerName)
                .HasMaxLength(255);

            builder.Property(b => b.CoverImageUrl)
                .HasMaxLength(2048);

            builder.Ignore(b => b.PricingBreakdown);
            builder.Ignore(b => b.ImageUrls);

            builder.HasIndex(b => new { b.PartnerCarId, b.StartTime, b.EndTime })
                .HasDatabaseName("idx_booking_car_time");

            builder.HasIndex(b => b.UserId)
                .HasDatabaseName("idx_booking_user");

            builder.HasIndex(b => b.PartnerUserId)
                .HasDatabaseName("idx_booking_partner_user");
        }

        private static BookingStatus ParseBookingStatus(string value)
        {
            return Enum.TryParse<BookingStatus>(value, true, out var parsed)
                ? parsed
                : BookingStatus.Pending;
        }
    }
}
