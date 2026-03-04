using CarService.Application.DTOs.CarModels;
using CarService.Application.DTOs.Common;

namespace CarService.Application.Interfaces
{
    public interface ICarModelService
    {
        Task<PagedResult<CarModelResponseDto>> GetAllAsync(CarModelQueryParams queryParams, CancellationToken cancellationToken = default);
        Task<CarModelDetailsDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<CarModelResponseDto> CreateAsync(CarModelCreateDto dto, CancellationToken cancellationToken = default);
        Task<CarModelResponseDto?> UpdateAsync(int id, CarModelUpdateDto dto, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task RecalculateRatingAsync(int modelId, CancellationToken cancellationToken = default);
    }
}
