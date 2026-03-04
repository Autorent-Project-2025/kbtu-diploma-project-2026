using CarService.Application.DTOs.CarComment;
using CarService.Application.DTOs.Common;
using CarService.Application.Interfaces;
using CarService.Domain.Entities;
using CarService.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace CarService.Infrastructure.Services
{
    public sealed class CarCommentService : ICarCommentService
    {
        private readonly ApplicationDbContext _db;

        public CarCommentService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<PagedResult<CarCommentResponseDto>> GetByPartnerCarPaginatedAsync(
            int partnerCarId,
            PaginationParams paginationParams,
            CancellationToken cancellationToken = default)
        {
            var query = _db.CarComments
                .AsNoTracking()
                .Where(comment => comment.PartnerCarId == partnerCarId);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(comment => comment.CreatedOn)
                .ThenByDescending(comment => comment.Id)
                .Skip((paginationParams.Page - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .Select(comment => MapToDto(comment))
                .ToListAsync(cancellationToken);

            return new PagedResult<CarCommentResponseDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = paginationParams.Page,
                PageSize = paginationParams.PageSize
            };
        }

        public async Task<CarCommentResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _db.CarComments
                .AsNoTracking()
                .FirstOrDefaultAsync(comment => comment.Id == id, cancellationToken);

            return entity is null ? null : MapToDto(entity);
        }

        public async Task<CarCommentResponseDto> CreateAsync(
            string userId,
            string userName,
            CarCommentCreateDto dto,
            CancellationToken cancellationToken = default)
        {
            var partnerCar = await _db.PartnerCars
                .FirstOrDefaultAsync(car => car.Id == dto.PartnerCarId, cancellationToken);

            if (partnerCar is null)
            {
                throw new KeyNotFoundException($"Partner car with id {dto.PartnerCarId} was not found.");
            }

            var entity = new CarComment
            {
                UserId = NormalizeRequired(userId, nameof(userId), 64),
                UserName = NormalizeRequired(userName, nameof(userName), 255),
                CarId = partnerCar.CarModelId,
                PartnerCarId = partnerCar.Id,
                Content = NormalizeRequired(dto.Content, nameof(dto.Content), 4000),
                Rating = dto.Rating,
                CreatedOn = DateTime.UtcNow
            };

            _db.CarComments.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);

            await RecalculateRatingsAsync(partnerCar.Id, partnerCar.CarModelId, cancellationToken);

            return MapToDto(entity);
        }

        public async Task<CarCommentResponseDto?> UpdateAsync(
            string userId,
            int commentId,
            CarCommentUpdateDto dto,
            CancellationToken cancellationToken = default)
        {
            var entity = await _db.CarComments
                .FirstOrDefaultAsync(comment => comment.Id == commentId, cancellationToken);

            if (entity is null)
            {
                return null;
            }

            if (!string.Equals(entity.UserId, userId, StringComparison.Ordinal))
            {
                throw new UnauthorizedAccessException("You are not authorized to update this comment.");
            }

            entity.Content = NormalizeRequired(dto.Content, nameof(dto.Content), 4000);
            entity.Rating = dto.Rating;

            await _db.SaveChangesAsync(cancellationToken);

            if (entity.PartnerCarId.HasValue)
            {
                await RecalculateRatingsAsync(entity.PartnerCarId.Value, entity.CarId, cancellationToken);
            }
            else
            {
                await RecalculateModelRatingOnlyAsync(entity.CarId, cancellationToken);
            }

            return MapToDto(entity);
        }

        public async Task<bool> DeleteAsync(string userId, int commentId, CancellationToken cancellationToken = default)
        {
            var entity = await _db.CarComments
                .FirstOrDefaultAsync(comment => comment.Id == commentId, cancellationToken);

            if (entity is null)
            {
                return false;
            }

            if (!string.Equals(entity.UserId, userId, StringComparison.Ordinal))
            {
                throw new UnauthorizedAccessException("You are not authorized to delete this comment.");
            }

            var partnerCarId = entity.PartnerCarId;
            var modelId = entity.CarId;

            _db.CarComments.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);

            if (partnerCarId.HasValue)
            {
                await RecalculateRatingsAsync(partnerCarId.Value, modelId, cancellationToken);
            }
            else
            {
                await RecalculateModelRatingOnlyAsync(modelId, cancellationToken);
            }

            return true;
        }

        private async Task RecalculateRatingsAsync(int partnerCarId, int modelId, CancellationToken cancellationToken)
        {
            var partnerCar = await _db.PartnerCars.FirstOrDefaultAsync(car => car.Id == partnerCarId, cancellationToken);
            if (partnerCar is not null)
            {
                var partnerRatings = await _db.CarComments
                    .Where(comment => comment.PartnerCarId == partnerCarId)
                    .Select(comment => comment.Rating)
                    .ToListAsync(cancellationToken);

                partnerCar.RatingsCount = partnerRatings.Count;
                partnerCar.Rating = partnerRatings.Count == 0
                    ? null
                    : Math.Round((decimal)partnerRatings.Average(), 1, MidpointRounding.AwayFromZero);
            }

            await RecalculateModelRatingOnlyAsync(modelId, cancellationToken);
        }

        private async Task RecalculateModelRatingOnlyAsync(int modelId, CancellationToken cancellationToken)
        {
            var model = await _db.CarModels.FirstOrDefaultAsync(entity => entity.Id == modelId, cancellationToken);
            if (model is null)
            {
                return;
            }

            var ratings = await _db.CarComments
                .Where(comment => comment.CarId == modelId)
                .Select(comment => comment.Rating)
                .ToListAsync(cancellationToken);

            model.RatingsCount = ratings.Count;
            model.Rating = ratings.Count == 0
                ? null
                : Math.Round((decimal)ratings.Average(), 1, MidpointRounding.AwayFromZero);

            await _db.SaveChangesAsync(cancellationToken);
        }

        private static CarCommentResponseDto MapToDto(CarComment entity)
        {
            return new CarCommentResponseDto
            {
                Id = entity.Id,
                UserId = entity.UserId,
                UserName = entity.UserName,
                CarId = entity.CarId,
                PartnerCarId = entity.PartnerCarId,
                Content = entity.Content,
                Rating = entity.Rating,
                CreatedOn = entity.CreatedOn
            };
        }

        private static string NormalizeRequired(string? value, string paramName, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"{paramName} is required.", paramName);
            }

            var normalized = value.Trim();
            if (normalized.Length > maxLength)
            {
                throw new ArgumentException($"{paramName} length must not exceed {maxLength}.", paramName);
            }

            return normalized;
        }
    }
}
