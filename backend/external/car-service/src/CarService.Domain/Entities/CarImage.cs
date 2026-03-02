using CarService.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarService.Domain.Entities
{
    public class CarImage
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("car_id")]
        public required int CarId { get; set; }

        [Column("image_url")]
        public required string ImageUrl { get; set; }

        [Column("image_type")]
        public required CarImageType ImageType { get; set; }

        [Column("display_order")]
        public required int DisplayOrder { get; set; }

        [ForeignKey(nameof(CarId))]
        public Car Car { get; set; } = null!;
    }
}
