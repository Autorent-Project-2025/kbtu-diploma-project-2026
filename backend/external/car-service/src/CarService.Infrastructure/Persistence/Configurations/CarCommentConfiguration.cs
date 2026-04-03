using CarService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarService.Infrastructure.Persistence.Configurations
{
    public class CarCommentConfiguration : IEntityTypeConfiguration<CarComment>
    {
        public void Configure(EntityTypeBuilder<CarComment> builder)
        {
            builder.ToTable("car_comments");

            builder.Property(comment => comment.UserId)
                .HasMaxLength(64);

            builder.HasOne(comment => comment.Car)
                .WithMany(model => model.Comments)
                .HasForeignKey(comment => comment.CarId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(comment => comment.PartnerCar)
                .WithMany(partnerCar => partnerCar.Comments)
                .HasForeignKey(comment => comment.PartnerCarId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
