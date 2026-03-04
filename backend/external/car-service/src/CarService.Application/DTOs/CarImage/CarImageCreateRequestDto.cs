using CarService.Application.Attributes;
using CarService.Domain.Enums;

namespace CarService.Application.DTOs.CarImage
{
    public class CarImageCreateRequestDto
    {
        public string Base64Content { get; set; } = string.Empty;

        [EnumName(typeof(CarImageType))]
        public CarImageType ImageType { get; set; }

        public int DisplayOrder { get; set; }
    }
}
