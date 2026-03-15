using ClientService.Application.DTOs;

namespace ClientService.Application.Interfaces;

public interface IClientService
{
    Task<IReadOnlyCollection<ClientResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ClientResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ClientResponseDto> CreateAsync(ClientCreateDto dto, CancellationToken cancellationToken = default);
    Task<ClientResponseDto?> UpdateAsync(int id, ClientUpdateDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    // Profile endpoints — operate by relatedUserId from JWT
    Task<ClientResponseDto?> GetByRelatedUserIdAsync(string relatedUserId, CancellationToken cancellationToken = default);
    Task<ClientResponseDto?> UpdateByRelatedUserIdAsync(string relatedUserId, ProfileUpdateDto dto, CancellationToken cancellationToken = default);
}
