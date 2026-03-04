using CarService.Application.DTOs.Common;
using CarService.Application.DTOs.PartnerCars;

namespace CarService.Application.Interfaces
{
    public interface IPartnerCarService
    {
        Task<PagedResult<PartnerCarResponseDto>> GetAllAsync(PartnerCarQueryParams queryParams, CancellationToken cancellationToken = default);
        Task<PartnerCarDetailsDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<PartnerCarResponseDto> CreateAsync(Guid currentUserId, PartnerCarCreateDto dto, CancellationToken cancellationToken = default);
        Task<PartnerCarResponseDto> ProvisionAsync(PartnerCarProvisionDto dto, CancellationToken cancellationToken = default);
        Task<PartnerCarResponseDto?> UpdateAsync(Guid currentUserId, int id, PartnerCarUpdateDto dto, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Guid currentUserId, int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<MyPartnerCarSummaryDto>> GetMyCarsAsync(Guid currentUserId, CancellationToken cancellationToken = default);
        Task<MyPartnerCarDetailsDto?> GetMyCarDetailsAsync(Guid currentUserId, int carId, CancellationToken cancellationToken = default);
        Task RecalculateRatingAsync(int partnerCarId, CancellationToken cancellationToken = default);
    }
}
