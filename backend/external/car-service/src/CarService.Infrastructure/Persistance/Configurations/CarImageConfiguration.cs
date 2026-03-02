using CarService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarService.Infrastructure.Persistence.Configurations
{
    public class CarImageConfiguration : IEntityTypeConfiguration<CarImage>
    {
        public void Configure(EntityTypeBuilder<CarImage> builder)
        {
            builder.ToTable("CarImage");

            builder.HasOne(e => e.Car)
                .WithMany(e => e.CarImages)
                .HasForeignKey(e => e.CarId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(b => b.ImageType)
                .HasConversion<string>();
        }
    }
}
