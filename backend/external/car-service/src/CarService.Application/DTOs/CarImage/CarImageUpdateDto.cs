using CarService.Application.Attributes;
using CarService.Domain.Enums;

namespace CarService.Application.DTOs.CarImage
{
    public class CarImageUpdateDto
    {
        public string? Base64Content { get; set; }

        [EnumName(typeof(CarImageType))]
        public CarImageType ImageType { get; set; }

        public int DisplayOrder { get; set; }
    }
}
