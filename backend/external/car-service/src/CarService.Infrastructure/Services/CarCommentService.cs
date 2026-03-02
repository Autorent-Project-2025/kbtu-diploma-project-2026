using CarService.Application.DTOs.CarComment;
using CarService.Application.DTOs.Common;
using CarService.Application.Interfaces;
using CarService.Application.Mappers;
using CarService.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace CarService.Infrastructure.Services
{
    public class CarCommentService : ICarCommentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICarService _carService;

        public CarCommentService(ApplicationDbContext context, ICarService carService)
        {
            _context = context;
            _carService = carService;
        }

        public async Task<PagedResult<CarCommentDetailsResponseDto>> GetByCarIdAllPaginated(int carId, PaginationParams paginationParams)
        {
            var query = _context.CarComments
                .AsNoTracking()
                .Where(c => c.CarId == carId);

            var totalCount = await query.CountAsync();

            var comments = await query
                .OrderByDescending(c => c.CreatedOn)
                .ThenByDescending(c => c.Id)
                .Skip((paginationParams.Page - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .SelectToCarCommentDetailsResponseDto()
                .ToListAsync();

            return new PagedResult<CarCommentDetailsResponseDto>
            {
                Items = comments,
                TotalCount = totalCount,
                Page = paginationParams.Page,
                PageSize = paginationParams.PageSize
            };
        }

        public async Task<CarCommentResponseDto> CreateAsync(int userId, CarCommentCreateDto dto)
        {
            var comment = dto.ToCarCommentEntity(userId, DateTime.UtcNow);

            _context.CarComments.Add(comment);
            await _context.SaveChangesAsync();
            await _carService.AdjustRating(comment.CarId);

            return comment.ToCarCommentResponseDto();
        }

        public async Task<CarCommentResponseDto> UpdateAsync(int userId, int commentId, CarCommentUpdateDto dto)
        {
            var comment = await _context.CarComments.FindAsync(commentId);

            if (comment == null)
            {
                throw new KeyNotFoundException($"Comment with ID {commentId} not found.");
            }

            if (comment.UserId != userId)
            {
                throw new UnauthorizedAccessException("You are not authorized to update this comment.");
            }

            comment.ApplyUpdate(dto);

            await _context.SaveChangesAsync();
            await _carService.AdjustRating(comment.CarId);

            return comment.ToCarCommentResponseDto();
        }

        public async Task DeleteAsync(int userId, int commentId)
        {
            var comment = await _context.CarComments.FindAsync(commentId);

            if (comment == null)
            {
                throw new KeyNotFoundException($"Comment with ID {commentId} not found.");
            }

            if (comment.UserId != userId)
            {
                throw new UnauthorizedAccessException("You are not authorized to delete this comment.");
            }

            _context.CarComments.Remove(comment);
            await _context.SaveChangesAsync();
        }
    }
}