using CarService.Application.DTOs.CarComment;
using CarService.Application.DTOs.Common;

namespace CarService.Application.Interfaces
{
    public interface ICarCommentService
    {
        Task<PagedResult<CarCommentResponseDto>> GetByPartnerCarPaginatedAsync(
            int partnerCarId,
            PaginationParams paginationParams,
            CancellationToken cancellationToken = default);
        Task<CarCommentResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<CarCommentResponseDto> CreateAsync(
            string userId,
            string userName,
            CarCommentCreateDto dto,
            CancellationToken cancellationToken = default);
        Task<CarCommentResponseDto?> UpdateAsync(
            string userId,
            int commentId,
            CarCommentUpdateDto dto,
            CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string userId, int commentId, CancellationToken cancellationToken = default);
    }
}
