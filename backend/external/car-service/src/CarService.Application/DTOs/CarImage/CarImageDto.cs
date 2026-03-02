using CarService.Application.Attributes;
using CarService.Domain.Enums;

namespace CarService.Application.DTOs.CarImage
{
    public class CarImageDto
    {
        public int Id { get; set; }

        public required string ImageUrl { get; set; }

        [EnumName(typeof(CarImageType))]
        public required CarImageType ImageType { get; set; }

        public required int DisplayOrder { get; set; }
    }
}
