using CarService.Application.DTOs.Bookings;
using CarService.Application.DTOs.CarComment;
using CarService.Application.DTOs.CarImage;
using CarService.Application.DTOs.Common;
using CarService.Application.DTOs.PartnerCars;
using CarService.Application.Interfaces;
using CarService.Application.Interfaces.Integrations;
using CarService.Domain.Entities;
using CarService.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace CarService.Infrastructure.Services
{
    public sealed class PartnerCarService : IPartnerCarService
    {
        private readonly ApplicationDbContext _db;
        private readonly IBookingReadClient _bookingReadClient;

        public PartnerCarService(
            ApplicationDbContext db,
            IBookingReadClient bookingReadClient)
        {
            _db = db;
            _bookingReadClient = bookingReadClient;
        }

        public async Task<PagedResult<PartnerCarResponseDto>> GetAllAsync(
            PartnerCarQueryParams queryParams,
            CancellationToken cancellationToken = default)
        {
            IQueryable<PartnerCar> query = _db.PartnerCars
                .AsNoTracking()
                .Include(partnerCar => partnerCar.CarModel);

            if (queryParams.CarModelId.HasValue)
            {
                query = query.Where(partnerCar => partnerCar.CarModelId == queryParams.CarModelId.Value);
            }

            if (queryParams.Status.HasValue)
            {
                query = query.Where(partnerCar => partnerCar.Status == queryParams.Status.Value);
            }

            if (queryParams.PartnerId.HasValue)
            {
                query = query.Where(partnerCar => partnerCar.PartnerId == queryParams.PartnerId.Value);
            }

            query = query
                .OrderByDescending(partnerCar => partnerCar.CreatedAt)
                .ThenByDescending(partnerCar => partnerCar.Id);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((queryParams.Page - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .Select(partnerCar => MapToResponse(partnerCar))
                .ToListAsync(cancellationToken);

            return new PagedResult<PartnerCarResponseDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = queryParams.Page,
                PageSize = queryParams.PageSize
            };
        }

        public async Task<PartnerCarDetailsDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _db.PartnerCars
                .AsNoTracking()
                .Include(partnerCar => partnerCar.CarModel)
                .Include(partnerCar => partnerCar.Images)
                .Include(partnerCar => partnerCar.Comments)
                .FirstOrDefaultAsync(partnerCar => partnerCar.Id == id, cancellationToken);

            if (entity is null)
            {
                return null;
            }

            var bookings = await _bookingReadClient.GetBookingsByCarIdAsync(id, cancellationToken);

            return new PartnerCarDetailsDto
            {
                Id = entity.Id,
                PartnerId = entity.PartnerId,
                CarModelId = entity.CarModelId,
                LicensePlate = entity.LicensePlate,
                Color = entity.Color,
                PriceHour = entity.PriceHour,
                PriceDay = entity.PriceDay,
                Status = entity.Status,
                CreatedAt = entity.CreatedAt,
                Rating = entity.Rating,
                RatingsCount = entity.RatingsCount,
                ModelBrand = entity.CarModel.Brand,
                ModelName = entity.CarModel.Model,
                ModelYear = entity.CarModel.Year,
                Images = entity.Images
                    .OrderBy(image => image.DisplayOrder)
                    .ThenBy(image => image.Id)
                    .Select(MapToImageDto)
                    .ToList(),
                Comments = entity.Comments
                    .OrderByDescending(comment => comment.CreatedOn)
                    .ThenByDescending(comment => comment.Id)
                    .Select(MapToCommentDto)
                    .ToList(),
                Bookings = bookings.OrderByDescending(booking => booking.StartDate).ToList()
            };
        }

        public async Task<PartnerCarResponseDto> CreateAsync(
            Guid currentUserId,
            PartnerCarCreateDto dto,
            CancellationToken cancellationToken = default)
        {
            if (!await _db.CarModels.AnyAsync(model => model.Id == dto.CarModelId, cancellationToken))
            {
                throw new KeyNotFoundException($"Car model with id {dto.CarModelId} was not found.");
            }

            var entity = new PartnerCar
            {
                PartnerId = currentUserId,
                CarModelId = dto.CarModelId,
                LicensePlate = NormalizeRequired(dto.LicensePlate, nameof(dto.LicensePlate), 20),
                Color = NormalizeOptional(dto.Color, 50),
                PriceHour = dto.PriceHour,
                PriceDay = dto.PriceDay,
                Status = dto.Status,
                CreatedAt = DateTimeOffset.UtcNow,
                RatingsCount = 0
            };

            _db.PartnerCars.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);

            await _db.Entry(entity).Reference(partnerCar => partnerCar.CarModel).LoadAsync(cancellationToken);

            return MapToResponse(entity);
        }

        public async Task<PartnerCarResponseDto?> UpdateAsync(
            Guid currentUserId,
            int id,
            PartnerCarUpdateDto dto,
            CancellationToken cancellationToken = default)
        {
            var entity = await _db.PartnerCars
                .Include(partnerCar => partnerCar.CarModel)
                .FirstOrDefaultAsync(partnerCar => partnerCar.Id == id, cancellationToken);

            if (entity is null)
            {
                return null;
            }

            if (entity.PartnerId != currentUserId)
            {
                throw new UnauthorizedAccessException("You are not allowed to update this partner car.");
            }

            entity.LicensePlate = NormalizeRequired(dto.LicensePlate, nameof(dto.LicensePlate), 20);
            entity.Color = NormalizeOptional(dto.Color, 50);
            entity.PriceHour = dto.PriceHour;
            entity.PriceDay = dto.PriceDay;
            entity.Status = dto.Status;

            await _db.SaveChangesAsync(cancellationToken);

            return MapToResponse(entity);
        }

        public async Task<bool> DeleteAsync(Guid currentUserId, int id, CancellationToken cancellationToken = default)
        {
            var entity = await _db.PartnerCars.FirstOrDefaultAsync(partnerCar => partnerCar.Id == id, cancellationToken);
            if (entity is null)
            {
                return false;
            }

            if (entity.PartnerId != currentUserId)
            {
                throw new UnauthorizedAccessException("You are not allowed to delete this partner car.");
            }

            _db.PartnerCars.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<IReadOnlyCollection<MyPartnerCarSummaryDto>> GetMyCarsAsync(
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            var cars = await _db.PartnerCars
                .AsNoTracking()
                .Include(partnerCar => partnerCar.CarModel)
                .Where(partnerCar => partnerCar.PartnerId == currentUserId)
                .OrderByDescending(partnerCar => partnerCar.CreatedAt)
                .ThenByDescending(partnerCar => partnerCar.Id)
                .ToListAsync(cancellationToken);

            var ids = cars.Select(car => car.Id).ToArray();
            var counts = await _bookingReadClient.GetBookingCountsByCarIdsAsync(ids, cancellationToken);
            var countsMap = counts.ToDictionary(item => item.CarId, item => item.Count);

            return cars.Select(car => new MyPartnerCarSummaryDto
                {
                    Id = car.Id,
                    ModelDisplayName = $"{car.CarModel.Brand} {car.CarModel.Model} {car.CarModel.Year}",
                    Rating = car.Rating,
                    BookingCount = countsMap.GetValueOrDefault(car.Id, 0),
                    LicensePlate = car.LicensePlate,
                    PriceHour = car.PriceHour,
                    PriceDay = car.PriceDay,
                    Color = car.Color
                })
                .ToList();
        }

        public async Task<MyPartnerCarDetailsDto?> GetMyCarDetailsAsync(
            Guid currentUserId,
            int carId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _db.PartnerCars
                .AsNoTracking()
                .Include(partnerCar => partnerCar.CarModel)
                .Include(partnerCar => partnerCar.Images)
                .Include(partnerCar => partnerCar.Comments)
                .FirstOrDefaultAsync(partnerCar => partnerCar.Id == carId && partnerCar.PartnerId == currentUserId, cancellationToken);

            if (entity is null)
            {
                return null;
            }

            var bookings = await _bookingReadClient.GetBookingsByCarIdAsync(carId, cancellationToken);

            return new MyPartnerCarDetailsDto
            {
                Id = entity.Id,
                PartnerId = entity.PartnerId,
                LicensePlate = entity.LicensePlate,
                Color = entity.Color,
                PriceHour = entity.PriceHour,
                PriceDay = entity.PriceDay,
                Status = entity.Status,
                CreatedAt = entity.CreatedAt,
                Rating = entity.Rating,
                RatingsCount = entity.RatingsCount,
                ModelId = entity.CarModel.Id,
                Brand = entity.CarModel.Brand,
                Model = entity.CarModel.Model,
                Year = entity.CarModel.Year,
                Engine = entity.CarModel.Engine,
                Transmission = entity.CarModel.Transmission,
                Seats = entity.CarModel.Seats,
                FuelType = entity.CarModel.FuelType,
                Doors = entity.CarModel.Doors,
                Description = entity.CarModel.Description,
                Images = entity.Images
                    .OrderBy(image => image.DisplayOrder)
                    .ThenBy(image => image.Id)
                    .Select(MapToImageDto)
                    .ToList(),
                Comments = entity.Comments
                    .OrderByDescending(comment => comment.CreatedOn)
                    .ThenByDescending(comment => comment.Id)
                    .Select(MapToCommentDto)
                    .ToList(),
                Bookings = bookings
                    .OrderByDescending(booking => booking.StartDate)
                    .ToList()
            };
        }

        public async Task RecalculateRatingAsync(int partnerCarId, CancellationToken cancellationToken = default)
        {
            var entity = await _db.PartnerCars.FirstOrDefaultAsync(car => car.Id == partnerCarId, cancellationToken);
            if (entity is null)
            {
                return;
            }

            var ratings = await _db.CarComments
                .Where(comment => comment.PartnerCarId == partnerCarId)
                .Select(comment => comment.Rating)
                .ToListAsync(cancellationToken);

            entity.RatingsCount = ratings.Count;
            entity.Rating = ratings.Count == 0
                ? null
                : Math.Round((decimal)ratings.Average(), 1, MidpointRounding.AwayFromZero);

            await _db.SaveChangesAsync(cancellationToken);
        }

        private static PartnerCarResponseDto MapToResponse(PartnerCar entity)
        {
            return new PartnerCarResponseDto
            {
                Id = entity.Id,
                PartnerId = entity.PartnerId,
                CarModelId = entity.CarModelId,
                LicensePlate = entity.LicensePlate,
                Color = entity.Color,
                PriceHour = entity.PriceHour,
                PriceDay = entity.PriceDay,
                Status = entity.Status,
                CreatedAt = entity.CreatedAt,
                Rating = entity.Rating,
                RatingsCount = entity.RatingsCount,
                ModelBrand = entity.CarModel.Brand,
                ModelName = entity.CarModel.Model,
                ModelYear = entity.CarModel.Year
            };
        }

        private static CarImageDto MapToImageDto(PartnerCarImage image)
        {
            return new CarImageDto
            {
                Id = image.Id,
                ImageId = image.ImageId,
                ImageUrl = image.ImageUrl,
                ImageType = image.ImageType,
                DisplayOrder = image.DisplayOrder
            };
        }

        private static CarCommentResponseDto MapToCommentDto(CarComment comment)
        {
            return new CarCommentResponseDto
            {
                Id = comment.Id,
                UserId = comment.UserId,
                UserName = comment.UserName,
                CarId = comment.CarId,
                PartnerCarId = comment.PartnerCarId,
                Content = comment.Content,
                Rating = comment.Rating,
                CreatedOn = comment.CreatedOn
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

        private static string? NormalizeOptional(string? value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var normalized = value.Trim();
            if (normalized.Length > maxLength)
            {
                throw new ArgumentException($"Value length must not exceed {maxLength}.");
            }

            return normalized;
        }
    }
}
