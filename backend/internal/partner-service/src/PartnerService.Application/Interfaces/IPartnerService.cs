using PartnerService.Application.DTOs;

namespace PartnerService.Application.Interfaces;

public interface IPartnerService
{
    Task<IReadOnlyCollection<PartnerResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PartnerResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PartnerResponseDto?> GetByRelatedUserIdAsync(string relatedUserId, CancellationToken cancellationToken = default);
    Task<PartnerResponseDto> CreateAsync(PartnerCreateDto dto, CancellationToken cancellationToken = default);
    Task<PartnerResponseDto?> UpdateAsync(int id, PartnerUpdateDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
