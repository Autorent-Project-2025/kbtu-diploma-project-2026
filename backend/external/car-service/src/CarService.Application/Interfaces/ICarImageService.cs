using CarService.Application.DTOs.CarImage;

namespace CarService.Application.Interfaces
{
    public interface ICarImageService
    {
        Task<CarImageCreateResponseDto> Create(int carId, CarImageCreateRequestDto dto);
        Task<IEnumerable<CarImageDto>> GetByCarId(int carId);
    }
}
