using ClientService.Application.DTOs;

namespace ClientService.Application.Interfaces;

public interface IClientService
{
    Task<IReadOnlyCollection<ClientResponseDto>> GetAllAsync(string? search = null, CancellationToken cancellationToken = default);
    Task<ClientResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ClientResponseDto> CreateAsync(ClientCreateDto dto, CancellationToken cancellationToken = default);
    Task<ClientResponseDto?> UpdateAsync(
        int id,
        ClientUpdateDto dto,
        string authorizationHeader,
        CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    // Profile endpoints — operate by relatedUserId from JWT
    Task<ClientResponseDto?> GetByRelatedUserIdAsync(string relatedUserId, CancellationToken cancellationToken = default);
    Task<ClientResponseDto?> UpdateByRelatedUserIdAsync(
        string relatedUserId,
        ProfileUpdateDto dto,
        string authorizationHeader,
        CancellationToken cancellationToken = default);
    Task<ClientBookingAccessDto?> GetBookingAccessByRelatedUserIdAsync(string relatedUserId, CancellationToken cancellationToken = default);
    Task<ClientResponseDto?> SetBookingActionsBlockedByRelatedUserIdAsync(
        string relatedUserId,
        bool isBlocked,
        string? reason,
        CancellationToken cancellationToken = default);
}
