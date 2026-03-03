using ClientService.Application.DTOs;

namespace ClientService.Application.Interfaces;

public interface IClientService
{
    Task<IReadOnlyCollection<ClientResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ClientResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ClientResponseDto> CreateAsync(ClientCreateDto dto, CancellationToken cancellationToken = default);
    Task<ClientResponseDto?> UpdateAsync(int id, ClientUpdateDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
