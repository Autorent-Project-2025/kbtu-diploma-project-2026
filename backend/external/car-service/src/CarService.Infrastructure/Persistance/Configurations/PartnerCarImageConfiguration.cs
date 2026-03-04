using CarService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarService.Infrastructure.Persistence.Configurations
{
    public class PartnerCarImageConfiguration : IEntityTypeConfiguration<PartnerCarImage>
    {
        public void Configure(EntityTypeBuilder<PartnerCarImage> builder)
        {
            builder.ToTable("partner_car_images");

            builder.HasOne(image => image.Car)
                .WithMany(partnerCar => partnerCar.Images)
                .HasForeignKey(image => image.CarId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
