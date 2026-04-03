using CarService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarService.Infrastructure.Persistence.Configurations
{
    public class CarBrandConfiguration : IEntityTypeConfiguration<CarBrand>
    {
        public void Configure(EntityTypeBuilder<CarBrand> builder)
        {
            builder.ToTable("brands");

            builder.Property(entity => entity.Name)
                .HasMaxLength(255)
                .IsRequired();

            builder.HasIndex(entity => entity.Name)
                .IsUnique();
        }
    }
}
