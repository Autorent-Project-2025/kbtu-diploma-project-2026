using CarService.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarService.Domain.Entities
{
    public class PartnerCarImage
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("car_id")]
        public int CarId { get; set; }

        [Column("image_url")]
        public string ImageUrl { get; set; } = string.Empty;

        [Column("image_id")]
        public string? ImageId { get; set; }

        [Column("image_type")]
        public CarImageType ImageType { get; set; }

        [Column("display_order")]
        public int DisplayOrder { get; set; }

        public PartnerCar Car { get; set; } = null!;
    }
}
