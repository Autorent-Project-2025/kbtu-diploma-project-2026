using CarService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarService.Infrastructure.Persistence.Configurations
{
    public class CarModelLookupConfiguration : IEntityTypeConfiguration<CarModelLookup>
    {
        public void Configure(EntityTypeBuilder<CarModelLookup> builder)
        {
            builder.ToTable("models");

            builder.Property(entity => entity.Name)
                .HasMaxLength(255)
                .IsRequired();

            builder.HasOne(entity => entity.Brand)
                .WithMany(brand => brand.Models)
                .HasForeignKey(entity => entity.BrandId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(entity => new { entity.BrandId, entity.Name })
                .IsUnique();
        }
    }
}
