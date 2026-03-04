using CarService.Application.DTOs.Bookings;
using CarService.Application.DTOs.CarComment;
using CarService.Application.DTOs.CarImage;
using CarService.Application.DTOs.Common;
using CarService.Application.DTOs.Matching;
using CarService.Application.DTOs.PartnerCars;
using CarService.Application.Interfaces;
using CarService.Application.Interfaces.Integrations;
using CarService.Domain.Entities;
using CarService.Domain.Enums;
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
                OwnershipFileName = entity.OwnershipFileName,
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
                OwnershipFileName = null,
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

        public async Task<PartnerCarResponseDto> ProvisionAsync(
            PartnerCarProvisionDto dto,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dto);

            if (dto.RelatedUserId == Guid.Empty)
            {
                throw new ArgumentException($"{nameof(dto.RelatedUserId)} is required.", nameof(dto.RelatedUserId));
            }

            var normalizedBrand = NormalizeRequired(dto.CarBrand, nameof(dto.CarBrand), 100);
            var normalizedModel = NormalizeRequired(dto.CarModel, nameof(dto.CarModel), 100);
            var normalizedLicensePlate = NormalizeRequired(dto.LicensePlate, nameof(dto.LicensePlate), 20).ToUpperInvariant();
            var normalizedOwnershipFileName = NormalizeRequired(dto.OwnershipFileName, nameof(dto.OwnershipFileName), 255);

            var model = await _db.CarModels
                .AsNoTracking()
                .Where(carModel =>
                    carModel.Brand.ToLower() == normalizedBrand.ToLower() &&
                    carModel.Model.ToLower() == normalizedModel.ToLower())
                .OrderByDescending(carModel => carModel.Year)
                .ThenByDescending(carModel => carModel.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (model is null)
            {
                throw new KeyNotFoundException(
                    $"Car model '{normalizedBrand} {normalizedModel}' was not found.");
            }

            var images = (dto.Images ?? [])
                .Select((image, index) => new PartnerCarImage
                {
                    ImageId = NormalizeRequired(image.ImageId, nameof(image.ImageId), 255),
                    ImageUrl = NormalizeImageUrl(image.ImageUrl, nameof(image.ImageUrl)),
                    ImageType = CarImageType.General,
                    DisplayOrder = index + 1
                })
                .ToList();

            if (images.Count == 0)
            {
                throw new ArgumentException("At least one image is required.", nameof(dto.Images));
            }

            var entity = new PartnerCar
            {
                PartnerId = dto.RelatedUserId,
                CarModelId = model.Id,
                LicensePlate = normalizedLicensePlate,
                OwnershipFileName = normalizedOwnershipFileName,
                Status = PartnerCarStatus.Available,
                CreatedAt = DateTimeOffset.UtcNow,
                RatingsCount = 0
            };

            _db.PartnerCars.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);

            foreach (var image in images)
            {
                image.CarId = entity.Id;
            }

            _db.PartnerCarImages.AddRange(images);
            await _db.SaveChangesAsync(cancellationToken);

            entity.CarModel = await _db.CarModels.FirstAsync(carModel => carModel.Id == entity.CarModelId, cancellationToken);
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
                    OwnershipFileName = car.OwnershipFileName,
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
                OwnershipFileName = entity.OwnershipFileName,
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

        public async Task<IReadOnlyCollection<AvailableCarModelDto>> GetAvailableModelsAsync(CancellationToken cancellationToken = default)
        {
            var availableCars = await _db.PartnerCars
                .AsNoTracking()
                .Include(partnerCar => partnerCar.CarModel)
                .Where(partnerCar => partnerCar.Status == PartnerCarStatus.Available)
                .Select(partnerCar => new
                {
                    partnerCar.CarModelId,
                    partnerCar.PriceHour,
                    partnerCar.Rating,
                    Brand = partnerCar.CarModel.Brand,
                    Model = partnerCar.CarModel.Model,
                    Year = partnerCar.CarModel.Year
                })
                .ToListAsync(cancellationToken);

            return availableCars
                .GroupBy(item => new { item.CarModelId, item.Brand, item.Model, item.Year })
                .OrderBy(group => group.Key.Brand)
                .ThenBy(group => group.Key.Model)
                .ThenByDescending(group => group.Key.Year)
                .Select(group =>
                {
                    var ratings = group
                        .Where(item => item.Rating.HasValue)
                        .Select(item => item.Rating!.Value)
                        .ToList();

                    var prices = group
                        .Where(item => item.PriceHour.HasValue)
                        .Select(item => item.PriceHour!.Value)
                        .ToList();

                    return new AvailableCarModelDto
                    {
                        ModelId = group.Key.CarModelId,
                        Brand = group.Key.Brand,
                        Model = group.Key.Model,
                        Year = group.Key.Year,
                        AvailableCarsCount = group.Count(),
                        MinPriceHour = prices.Count == 0 ? null : prices.Min(),
                        MaxPriceHour = prices.Count == 0 ? null : prices.Max(),
                        AverageRating = ratings.Count == 0
                            ? null
                            : Math.Round(ratings.Average(), 1, MidpointRounding.AwayFromZero)
                    };
                })
                .ToList();
        }

        public async Task<MatchPartnerCarResponseDto> MatchPartnerCarAsync(
            MatchPartnerCarRequestDto dto,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dto);

            if (dto.ModelId <= 0)
            {
                throw new ArgumentException($"{nameof(dto.ModelId)} is required.", nameof(dto.ModelId));
            }

            if (dto.EndTime <= dto.StartTime)
            {
                throw new ArgumentException($"{nameof(dto.EndTime)} must be greater than {nameof(dto.StartTime)}.");
            }

            var candidates = await _db.PartnerCars
                .AsNoTracking()
                .Include(partnerCar => partnerCar.CarModel)
                .Where(partnerCar =>
                    partnerCar.CarModelId == dto.ModelId &&
                    partnerCar.Status == PartnerCarStatus.Available)
                .ToListAsync(cancellationToken);

            if (candidates.Count == 0)
            {
                return new MatchPartnerCarResponseDto
                {
                    IsAvailable = false
                };
            }

            var candidateIds = candidates.Select(candidate => candidate.Id).ToArray();
            var bookingCounts = await _bookingReadClient.GetBookingCountsByCarIdsAsync(candidateIds, cancellationToken);
            var bookingCountMap = bookingCounts.ToDictionary(item => item.CarId, item => item.Count);

            var availability = await _bookingReadClient.CheckAvailabilityByCarIdsAsync(
                candidateIds,
                dto.StartTime,
                dto.EndTime,
                cancellationToken);

            var availabilityMap = availability.ToDictionary(item => item.CarId);
            var availableCandidates = candidates
                .Where(candidate =>
                    availabilityMap.TryGetValue(candidate.Id, out var state) &&
                    state.IsAvailable)
                .ToList();

            if (availableCandidates.Count == 0)
            {
                var suggestedDates = availability
                    .Select(item => item.NextAvailableFrom)
                    .OrderBy(item => item)
                    .Distinct()
                    .Take(5)
                    .ToList();

                return new MatchPartnerCarResponseDto
                {
                    IsAvailable = false,
                    SuggestedStartTimesUtc = suggestedDates
                };
            }

            var partnerLoadMap = candidates
                .GroupBy(candidate => candidate.PartnerId)
                .ToDictionary(
                    group => group.Key,
                    group => group.Sum(candidate => bookingCountMap.GetValueOrDefault(candidate.Id, 0)));

            var minPartnerLoad = availableCandidates.Min(candidate => partnerLoadMap.GetValueOrDefault(candidate.PartnerId, 0));
            var maxPartnerLoad = availableCandidates.Max(candidate => partnerLoadMap.GetValueOrDefault(candidate.PartnerId, 0));
            var minBookingCount = availableCandidates.Min(candidate => bookingCountMap.GetValueOrDefault(candidate.Id, 0));
            var maxBookingCount = availableCandidates.Max(candidate => bookingCountMap.GetValueOrDefault(candidate.Id, 0));

            var priceValues = availableCandidates
                .Where(candidate => candidate.PriceHour.HasValue)
                .Select(candidate => (double)candidate.PriceHour!.Value)
                .ToList();

            var minPrice = priceValues.Count == 0 ? 0d : priceValues.Min();
            var maxPrice = priceValues.Count == 0 ? 0d : priceValues.Max();

            const double partnerLoadWeight = 0.35;
            const double ratingWeight = 0.30;
            const double bookingCountWeight = 0.20;
            const double priceWeight = 0.15;

            var rankedCandidates = availableCandidates
                .Select(candidate =>
                {
                    var partnerLoad = partnerLoadMap.GetValueOrDefault(candidate.PartnerId, 0);
                    var bookingCount = bookingCountMap.GetValueOrDefault(candidate.Id, 0);
                    var rating = candidate.Rating.HasValue ? Clamp01((double)candidate.Rating.Value / 5d) : 0d;

                    var partnerLoadScore = 1d - Normalize(partnerLoad, minPartnerLoad, maxPartnerLoad);
                    var bookingCountScore = Normalize(bookingCount, minBookingCount, maxBookingCount);
                    var priceScore = candidate.PriceHour.HasValue
                        ? 1d - Normalize((double)candidate.PriceHour.Value, minPrice, maxPrice)
                        : 0d;

                    var totalScore =
                        partnerLoadScore * partnerLoadWeight +
                        rating * ratingWeight +
                        bookingCountScore * bookingCountWeight +
                        priceScore * priceWeight;

                    return new
                    {
                        Candidate = candidate,
                        Score = totalScore
                    };
                })
                .OrderByDescending(item => item.Score)
                .ThenByDescending(item => item.Candidate.Rating ?? 0m)
                .ThenBy(item => item.Candidate.PriceHour ?? decimal.MaxValue)
                .ThenBy(item => item.Candidate.Id)
                .ToList();

            var selected = rankedCandidates[0].Candidate;

            return new MatchPartnerCarResponseDto
            {
                IsAvailable = true,
                PartnerCarId = selected.Id,
                PartnerId = selected.PartnerId,
                PriceHour = selected.PriceHour,
                ModelBrand = selected.CarModel.Brand,
                ModelName = selected.CarModel.Model,
                ModelYear = selected.CarModel.Year
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
                OwnershipFileName = entity.OwnershipFileName,
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

        private static string NormalizeImageUrl(string? value, string paramName)
        {
            var normalized = NormalizeRequired(value, paramName, 2048);
            if (!Uri.TryCreate(normalized, UriKind.Absolute, out _))
            {
                throw new ArgumentException($"{paramName} must be a valid absolute URL.", paramName);
            }

            return normalized;
        }

        private static double Normalize(double value, double min, double max)
        {
            if (max <= min)
            {
                return 1d;
            }

            return Clamp01((value - min) / (max - min));
        }

        private static double Normalize(int value, int min, int max)
        {
            if (max <= min)
            {
                return 1d;
            }

            return Clamp01((double)(value - min) / (max - min));
        }

        private static double Clamp01(double value)
        {
            if (value < 0d)
            {
                return 0d;
            }

            if (value > 1d)
            {
                return 1d;
            }

            return value;
        }
    }
}
