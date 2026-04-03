using CarService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarService.Infrastructure.Persistence.Configurations
{
    public class CarConfiguration : IEntityTypeConfiguration<Car>
    {
        public void Configure(EntityTypeBuilder<Car> builder)
        {
            builder.ToTable("car_models");

            builder.HasOne(model => model.Brand)
                .WithMany(brand => brand.CarModels)
                .HasForeignKey(model => model.BrandId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(model => model.ModelLookup)
                .WithMany(modelLookup => modelLookup.CarModels)
                .HasForeignKey(model => model.ModelId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(model => model.PartnerCars)
                .WithOne(partnerCar => partnerCar.CarModel)
                .HasForeignKey(partnerCar => partnerCar.CarModelId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(model => model.ModelImages)
                .WithOne(image => image.Model)
                .HasForeignKey(image => image.ModelId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(model => model.Comments)
                .WithOne(comment => comment.Car)
                .HasForeignKey(comment => comment.CarId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
