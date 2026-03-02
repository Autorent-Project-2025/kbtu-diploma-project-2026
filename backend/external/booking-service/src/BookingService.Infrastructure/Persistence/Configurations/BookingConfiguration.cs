using BookingService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingService.Infrastructure.Persistence.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.ToTable("bookings");

            builder.Property(b => b.UserId)
                .HasMaxLength(64);

            builder.Property(b => b.Status)
                .HasConversion<string>();

            builder.HasIndex(b => new { b.CarId, b.StartDate, b.EndDate });
        }
    }
}
