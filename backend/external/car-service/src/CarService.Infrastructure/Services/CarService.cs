using CarService.Application.DTOs.Cars;
using CarService.Application.DTOs.Common;
using CarService.Application.Mappers;
using CarService.Application.DTOs.Cars;
using CarService.Application.Entities;
using CarService.Application.Enums;
using CarService.Application.Interfaces;
using CarService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarService.Infrastructure.Services
{
    public class CarService : ICarService
    {
        private readonly ApplicationDbContext _db;

        public CarService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<CarResponseDto>> GetAll()
        {
            return await _db.Cars
                .SelectToCarResponseDtoWithDescription()
                .ToListAsync();
        }

        public async Task<PagedResult<CarResponseDto>> GetAllWithFiltersAndSorting(CarQueryParams queryParams)
        {
            var query = _db.Cars.AsQueryable();

            // Filtering
            if (!string.IsNullOrWhiteSpace(queryParams.Brand))
            {
                query = query.Where(c => c.Brand.ToLower().Contains(queryParams.Brand.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(queryParams.Model))
            {
                query = query.Where(c => c.Model.ToLower().Contains(queryParams.Model.ToLower()));
            }

            // Sorting
            var isDescending = queryParams.SortOrder?.ToLower() == "desc";

            query = queryParams.SortBy switch
            {
                CarSortOption.Rating => isDescending ? query.OrderByDescending(c => c.Rating) : query.OrderBy(c => c.Rating),
                CarSortOption.PriceHour => isDescending ? query.OrderByDescending(c => c.PriceHour) : query.OrderBy(c => c.PriceHour),
                CarSortOption.Year => isDescending ? query.OrderByDescending(c => c.Year) : query.OrderBy(c => c.Year),
                _ => query.OrderBy(c => c.Id)
            };

            var totalCount = await query.CountAsync();

            var cars = await query
                .Skip((queryParams.Page - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .SelectToCarResponseDto()
                .ToListAsync();

            return new PagedResult<CarResponseDto>
            {
                Items = cars,
                TotalCount = totalCount,
                Page = queryParams.Page,
                PageSize = queryParams.PageSize
            };
        }

        public async Task<CarDetailsResponseDto?> GetById(int id)
        {
            var car = await _db.Cars
                .Include(c => c.Comments)
                    .ThenInclude(cc => cc.User)
                .Include(c => c.CarFeatures)
                    .ThenInclude(cf => cf.Feature)
                .FirstOrDefaultAsync(c => c.Id == id);

            return car.ToCarDetailsResponseDto();
        }

        public async Task<CarCreateResponseDto> Create(CarCreateRequestDto dto)
        {
            var inputCarFeatureNames = dto.Features
                .Select(f => f.Name.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var existingCarFeatures = await _db.Features
                .Where(f => inputCarFeatureNames.Contains(f.Name))
                .ToListAsync();

            var existingCarFeatureNames = existingCarFeatures
                .Select(f => f.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var newFeaturesToCreate = inputCarFeatureNames
                .Where(name => !existingCarFeatureNames.Contains(name.ToLower()))
                .Select(name => new Feature
                {
                    Name = name,
                    CreatedOn = DateTime.UtcNow
                })
                .ToList();

            var allCarFeaturesToLink = new List<Feature>(existingCarFeatures);
            allCarFeaturesToLink.AddRange(newFeaturesToCreate);

            var car = dto.ToCarEntity(allCarFeaturesToLink);

            _db.Cars.Add(car);
            await _db.SaveChangesAsync();

            return car.ToCarCreateResponseDto();
        }

        public async Task<CommonResponseDto> Create(CarCreateRequestDto[] dtos)
        {
            var cars = dtos.Select(dto => dto.ToCarEntity()).ToList();

            _db.Cars.AddRange(cars);
            await _db.SaveChangesAsync();

            return new CommonResponseDto { Message = $"Successfully created {cars.Count} cars." };
        }

        public async Task<CarResponseDto?> Update(int id, CarUpdateDto dto)
        {
            var car = await _db.Cars.FindAsync(id);
            if (car == null) return null;

            car.ApplyUpdate(dto);

            await _db.SaveChangesAsync();

            return car.ToCarResponseDto();
        }

        public async Task<bool> Delete(int id)
        {
            var car = await _db.Cars.FindAsync(id);
            if (car == null) return false;

            _db.Cars.Remove(car);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task AdjustRating(int id)
        {
            var car = await _db.Cars
                .Include(c => c.Comments)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (!car.Comments.Any())
            {
                return;
            }

            car.Rating = (decimal)car.Comments.Average(c => c.Rating);
            await _db.SaveChangesAsync();
        }
    }
}