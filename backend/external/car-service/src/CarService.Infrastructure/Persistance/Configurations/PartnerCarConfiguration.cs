using CarService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarService.Infrastructure.Persistence.Configurations
{
    public class PartnerCarConfiguration : IEntityTypeConfiguration<PartnerCar>
    {
        public void Configure(EntityTypeBuilder<PartnerCar> builder)
        {
            builder.ToTable("partner_cars");

            builder.Property(partnerCar => partnerCar.ProvisionRequestKey)
                .HasMaxLength(128);

            builder.HasOne(partnerCar => partnerCar.CarModel)
                .WithMany(model => model.PartnerCars)
                .HasForeignKey(partnerCar => partnerCar.CarModelId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(partnerCar => partnerCar.Images)
                .WithOne(image => image.Car)
                .HasForeignKey(image => image.CarId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(partnerCar => partnerCar.Comments)
                .WithOne(comment => comment.PartnerCar)
                .HasForeignKey(comment => comment.PartnerCarId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(partnerCar => partnerCar.ProvisionRequestKey)
                .IsUnique();
        }
    }
}
