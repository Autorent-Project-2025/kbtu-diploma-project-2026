using CarService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarService.Infrastructure.Persistence.Configurations
{
    public class CarCommentConfiguration : IEntityTypeConfiguration<CarComment>
    {
        public void Configure(EntityTypeBuilder<CarComment> builder)
        {
            builder.ToTable("CarComment");

            builder.HasOne(e => e.Car)
                .WithMany(e => e.Comments)
                .HasForeignKey(e => e.CarId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
