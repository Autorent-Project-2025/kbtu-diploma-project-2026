using CarService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarService.Infrastructure.Persistence.Configurations
{
    public class CarFeatureConfiguration : IEntityTypeConfiguration<CarFeature>
    {
        public void Configure(EntityTypeBuilder<CarFeature> builder)
        {
            builder.ToTable("car_features");
            builder.HasIndex(cf => new { cf.CarId, cf.FeatureId }).IsUnique();

            builder.HasOne(cf => cf.Car)
                .WithMany(c => c.CarFeatures)
                .HasForeignKey(cf => cf.CarId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(cf => cf.Feature)
                .WithMany(f => f.CarFeatures)
                .HasForeignKey(cf => cf.FeatureId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
