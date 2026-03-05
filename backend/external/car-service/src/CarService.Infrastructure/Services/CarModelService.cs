using CarService.Application.DTOs.CarFeature;
using CarService.Application.DTOs.CarImage;
using CarService.Application.DTOs.CarModels;
using CarService.Application.DTOs.Common;
using CarService.Application.Interfaces;
using CarService.Domain.Entities;
using CarService.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace CarService.Infrastructure.Services
{
    public sealed class CarModelService : ICarModelService
    {
        private readonly ApplicationDbContext _db;
        private readonly CarCatalogResolver _catalogResolver;

        public CarModelService(
            ApplicationDbContext db,
            CarCatalogResolver catalogResolver)
        {
            _db = db;
            _catalogResolver = catalogResolver;
        }

        public async Task<PagedResult<CarModelResponseDto>> GetAllAsync(
            CarModelQueryParams queryParams,
            CancellationToken cancellationToken = default)
        {
            IQueryable<Car> query = _db.CarModels
                .AsNoTracking()
                .IncludeCatalog();

            if (!string.IsNullOrWhiteSpace(queryParams.Brand))
            {
                var brand = queryParams.Brand.Trim().ToLowerInvariant();
                query = query.Where(entity => entity.Brand.Name.ToLower().Contains(brand));
            }

            if (!string.IsNullOrWhiteSpace(queryParams.Model))
            {
                var model = queryParams.Model.Trim().ToLowerInvariant();
                query = query.Where(entity => entity.ModelLookup.Name.ToLower().Contains(model));
            }

            if (queryParams.Year.HasValue)
            {
                query = query.Where(entity => entity.Year == queryParams.Year.Value);
            }

            query = query
                .OrderBy(entity => entity.Brand.Name)
                .ThenBy(entity => entity.ModelLookup.Name)
                .ThenByDescending(entity => entity.Year)
                .ThenBy(entity => entity.Id);

            var totalCount = await query.CountAsync(cancellationToken);

            var entities = await query
                .Skip((queryParams.Page - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .ToListAsync(cancellationToken);

            var averagePricesByModelId = await GetAveragePricesByModelIdsAsync(
                entities.Select(entity => entity.Id).ToArray(),
                cancellationToken);

            var items = entities
                .Select(entity =>
                {
                    averagePricesByModelId.TryGetValue(entity.Id, out var averagePrices);
                    return MapToResponse(entity, averagePrices);
                })
                .ToList();

            return new PagedResult<CarModelResponseDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = queryParams.Page,
                PageSize = queryParams.PageSize
            };
        }

        public async Task<IReadOnlyCollection<string>> GetBrandsAsync(CancellationToken cancellationToken = default)
        {
            return await _db.CarBrands
                .AsNoTracking()
                .Select(brand => brand.Name)
                .Where(name => name != string.Empty)
                .OrderBy(name => name)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyCollection<string>> GetModelsAsync(
            string? brand,
            CancellationToken cancellationToken = default)
        {
            IQueryable<CarModelLookup> query = _db.CarModelLookups
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(brand))
            {
                var normalizedBrand = brand.Trim().ToLowerInvariant();
                query = query.Where(entity => entity.Brand.Name.ToLower() == normalizedBrand);
            }

            return await query
                .Select(entity => entity.Name)
                .Where(name => name != string.Empty)
                .OrderBy(name => name)
                .ToListAsync(cancellationToken);
        }

        public async Task<CarModelDetailsDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var model = await _db.CarModels
                .AsNoTracking()
                .IncludeCatalog()
                .Include(entity => entity.CarFeatures)
                    .ThenInclude(carFeature => carFeature.Feature)
                .Include(entity => entity.ModelImages)
                .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);

            if (model is null)
            {
                return null;
            }

            var averagePricesByModelId = await GetAveragePricesByModelIdsAsync([id], cancellationToken);
            averagePricesByModelId.TryGetValue(model.Id, out var averagePrices);

            return MapToDetails(model, averagePrices);
        }

        public async Task<CarModelResponseDto> CreateAsync(CarModelCreateDto dto, CancellationToken cancellationToken = default)
        {
            var (brand, model) = await _catalogResolver.ResolveAsync(dto.Brand, dto.Model, cancellationToken);

            var entity = new Car
            {
                BrandId = brand.Id,
                ModelId = model.Id,
                Year = dto.Year,
                Engine = NormalizeOptional(dto.Engine, 100),
                Transmission = NormalizeOptional(dto.Transmission, 100),
                Seats = dto.Seats,
                FuelType = NormalizeOptional(dto.FuelType, 50),
                Doors = dto.Doors,
                Description = NormalizeOptional(dto.Description, 585),
                RatingsCount = 0
            };

            _db.CarModels.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);

            entity.Brand = brand;
            entity.ModelLookup = model;
            return MapToResponse(entity, null);
        }

        public async Task<CarModelResponseDto?> UpdateAsync(
            int id,
            CarModelUpdateDto dto,
            CancellationToken cancellationToken = default)
        {
            var entity = await _db.CarModels.FirstOrDefaultAsync(model => model.Id == id, cancellationToken);
            if (entity is null)
            {
                return null;
            }

            var (brand, model) = await _catalogResolver.ResolveAsync(dto.Brand, dto.Model, cancellationToken);

            entity.BrandId = brand.Id;
            entity.ModelId = model.Id;
            entity.Year = dto.Year;
            entity.Engine = NormalizeOptional(dto.Engine, 100);
            entity.Transmission = NormalizeOptional(dto.Transmission, 100);
            entity.Seats = dto.Seats;
            entity.FuelType = NormalizeOptional(dto.FuelType, 50);
            entity.Doors = dto.Doors;
            entity.Description = NormalizeOptional(dto.Description, 585);

            await _db.SaveChangesAsync(cancellationToken);

            entity.Brand = brand;
            entity.ModelLookup = model;
            return MapToResponse(entity, null);
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _db.CarModels.FirstOrDefaultAsync(model => model.Id == id, cancellationToken);
            if (entity is null)
            {
                return false;
            }

            _db.CarModels.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task RecalculateRatingAsync(int modelId, CancellationToken cancellationToken = default)
        {
            var entity = await _db.CarModels.FirstOrDefaultAsync(model => model.Id == modelId, cancellationToken);
            if (entity is null)
            {
                return;
            }

            var comments = await _db.CarComments
                .Where(comment => comment.CarId == modelId)
                .Select(comment => comment.Rating)
                .ToListAsync(cancellationToken);

            entity.RatingsCount = comments.Count;
            entity.Rating = comments.Count == 0
                ? null
                : Math.Round((decimal)comments.Average(), 1, MidpointRounding.AwayFromZero);

            await _db.SaveChangesAsync(cancellationToken);
        }

        private async Task<Dictionary<int, CarModelAveragePrices>> GetAveragePricesByModelIdsAsync(
            IReadOnlyCollection<int> modelIds,
            CancellationToken cancellationToken)
        {
            if (modelIds.Count == 0)
            {
                return new Dictionary<int, CarModelAveragePrices>();
            }

            var aggregates = await _db.PartnerCars
                .AsNoTracking()
                .Where(partnerCar => modelIds.Contains(partnerCar.CarModelId))
                .GroupBy(partnerCar => partnerCar.CarModelId)
                .Select(group => new
                {
                    CarModelId = group.Key,
                    PriceHour = group.Average(partnerCar => partnerCar.PriceHour),
                    PriceDay = group.Average(partnerCar => partnerCar.PriceDay)
                })
                .ToListAsync(cancellationToken);

            return aggregates.ToDictionary(
                aggregate => aggregate.CarModelId,
                aggregate => new CarModelAveragePrices(
                    NormalizeAveragePrice(aggregate.PriceHour),
                    NormalizeAveragePrice(aggregate.PriceDay)));
        }

        private static CarModelResponseDto MapToResponse(Car entity, CarModelAveragePrices? averagePrices)
        {
            return new CarModelResponseDto
            {
                Id = entity.Id,
                Brand = entity.Brand.Name,
                Model = entity.ModelLookup.Name,
                Year = entity.Year,
                Engine = entity.Engine,
                Transmission = entity.Transmission,
                Seats = entity.Seats,
                FuelType = entity.FuelType,
                Doors = entity.Doors,
                Description = entity.Description,
                Rating = entity.Rating,
                RatingsCount = entity.RatingsCount,
                PriceHour = averagePrices?.PriceHour,
                PriceDay = averagePrices?.PriceDay
            };
        }

        private static CarModelDetailsDto MapToDetails(Car entity, CarModelAveragePrices? averagePrices)
        {
            return new CarModelDetailsDto
            {
                Id = entity.Id,
                Brand = entity.Brand.Name,
                Model = entity.ModelLookup.Name,
                Year = entity.Year,
                Engine = entity.Engine,
                Transmission = entity.Transmission,
                Seats = entity.Seats,
                FuelType = entity.FuelType,
                Doors = entity.Doors,
                Description = entity.Description,
                Rating = entity.Rating,
                RatingsCount = entity.RatingsCount,
                PriceHour = averagePrices?.PriceHour,
                PriceDay = averagePrices?.PriceDay,
                Features = entity.CarFeatures
                    .Select(carFeature => new CarFeatureDto
                    {
                        Name = carFeature.Feature.Name
                    })
                    .OrderBy(feature => feature.Name)
                    .ToList(),
                Images = entity.ModelImages
                    .OrderBy(image => image.DisplayOrder)
                    .ThenBy(image => image.Id)
                    .Select(image => new CarImageDto
                    {
                        Id = image.Id,
                        ImageId = image.ImageId,
                        ImageUrl = image.ImageUrl,
                        ImageType = image.ImageType,
                        DisplayOrder = image.DisplayOrder
                    })
                    .ToList()
            };
        }

        private static decimal? NormalizeAveragePrice(decimal? value)
        {
            if (!value.HasValue)
            {
                return null;
            }

            return Math.Round(value.Value, 2, MidpointRounding.AwayFromZero);
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

        private sealed record CarModelAveragePrices(decimal? PriceHour, decimal? PriceDay);
    }
}
