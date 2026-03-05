using CarService.Application.Attributes;
using CarService.Domain.Enums;

namespace CarService.Application.DTOs.CarImage
{
    public class CarImageDto
    {
        public int Id { get; set; }
        public string? ImageId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;

        [EnumName(typeof(CarImageType))]
        public CarImageType ImageType { get; set; }

        public int DisplayOrder { get; set; }
    }
}
