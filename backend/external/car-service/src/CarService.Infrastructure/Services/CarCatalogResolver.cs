using CarService.Domain.Entities;
using CarService.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace CarService.Infrastructure.Services
{
    public sealed class CarCatalogResolver
    {
        private readonly ApplicationDbContext _db;

        public CarCatalogResolver(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<(CarBrand Brand, CarModelLookup Model)> ResolveAsync(
            string brandName,
            string modelName,
            CancellationToken cancellationToken = default)
        {
            var normalizedBrand = NormalizeRequired(brandName, nameof(brandName), 255);
            var normalizedModel = NormalizeRequired(modelName, nameof(modelName), 255);
            var normalizedBrandKey = normalizedBrand.ToLowerInvariant();
            var normalizedModelKey = normalizedModel.ToLowerInvariant();

            var brand = await _db.CarBrands
                .FirstOrDefaultAsync(entity => entity.Name.ToLower() == normalizedBrandKey, cancellationToken);

            if (brand is null)
            {
                brand = new CarBrand
                {
                    Name = normalizedBrand
                };

                _db.CarBrands.Add(brand);
            }

            CarModelLookup? model = null;
            if (brand.Id > 0)
            {
                model = await _db.CarModelLookups
                    .FirstOrDefaultAsync(
                        entity => entity.BrandId == brand.Id && entity.Name.ToLower() == normalizedModelKey,
                        cancellationToken);
            }

            if (model is null)
            {
                model = brand.Id > 0
                    ? new CarModelLookup
                    {
                        BrandId = brand.Id,
                        Name = normalizedModel
                    }
                    : new CarModelLookup
                    {
                        Brand = brand,
                        Name = normalizedModel
                    };

                _db.CarModelLookups.Add(model);
            }

            if (_db.ChangeTracker.HasChanges())
            {
                await _db.SaveChangesAsync(cancellationToken);
            }

            return (brand, model);
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
