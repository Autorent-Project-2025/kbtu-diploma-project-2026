using CarService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarService.Infrastructure.Persistence.Configurations
{
    public class CarModelImageConfiguration : IEntityTypeConfiguration<CarModelImage>
    {
        public void Configure(EntityTypeBuilder<CarModelImage> builder)
        {
            builder.ToTable("car_model_images");

            builder.HasOne(image => image.Model)
                .WithMany(model => model.ModelImages)
                .HasForeignKey(image => image.ModelId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
