using CarService.Application.DTOs.CarFeature;
using CarService.Application.DTOs.CarImage;

namespace CarService.Application.DTOs.CarModels
{
    public class CarModelDetailsDto : CarModelResponseDto
    {
        public List<CarFeatureDto> Features { get; set; } = [];
        public List<CarImageDto> Images { get; set; } = [];
    }
}
