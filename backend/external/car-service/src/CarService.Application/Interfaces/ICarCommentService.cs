using CarService.Application.DTOs.CarComment;
using CarService.Application.DTOs.Common;

namespace CarService.Application.Interfaces
{
    public interface ICarCommentService
    {
        Task<PagedResult<CarCommentDetailsResponseDto>> GetByCarIdAllPaginated(int carId, PaginationParams paginationParams);
        Task<CarCommentResponseDto> CreateAsync(int userId, CarCommentCreateDto dto);
        Task<CarCommentResponseDto> UpdateAsync(int userId, int commentId, CarCommentUpdateDto dto);
        Task DeleteAsync(int userId, int commentId);
    }
}