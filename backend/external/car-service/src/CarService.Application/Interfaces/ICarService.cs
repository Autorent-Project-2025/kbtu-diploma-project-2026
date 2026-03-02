using CarService.Application.DTOs.Cars;
using CarService.Application.DTOs.Common;

namespace CarService.Application.Interfaces
{
    public interface ICarService
    {
        Task<IEnumerable<CarResponseDto>> GetAll();
        Task<PagedResult<CarResponseDto>> GetAllWithFiltersAndSorting(CarQueryParams queryParams);
        Task<CarDetailsResponseDto?> GetById(int id);
        Task<CarCreateResponseDto> Create(CarCreateRequestDto dto);
        Task<CommonResponseDto> Create(CarCreateRequestDto[] dtos);
        Task<CarResponseDto?> Update(int id, CarUpdateDto dto);
        Task<bool> Delete(int id);
        Task AdjustRating(int id);
    }
}
