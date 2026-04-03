using CarService.Application.Interfaces;
using CarService.Domain.Calculations;
using CarService.Domain.Entities;
using CarService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarService.Infrastructure.Services
{
    public sealed class PartnerCarDisplayPricingService : IPartnerCarDisplayPricingService
    {
        private readonly ApplicationDbContext _db;

        public PartnerCarDisplayPricingService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task RecalculateForCarModelAsync(
            int carModelId,
            CancellationToken cancellationToken = default)
        {
            var carModel = await _db.CarModels
                .FirstOrDefaultAsync(entity => entity.Id == carModelId, cancellationToken);

            if (carModel is null)
            {
                return;
            }

            var partnerCars = await _db.PartnerCars
                .Where(entity => entity.CarModelId == carModel.Id)
                .ToListAsync(cancellationToken);

            foreach (var partnerCar in partnerCars)
            {
                ApplyPricing(partnerCar, carModel.MarketValueKzt, carModel.Rating);
            }

            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task RecalculateForPartnerCarAsync(
            int partnerCarId,
            CancellationToken cancellationToken = default)
        {
            var partnerCar = await _db.PartnerCars
                .Include(entity => entity.CarModel)
                .FirstOrDefaultAsync(entity => entity.Id == partnerCarId, cancellationToken);

            if (partnerCar is null)
            {
                return;
            }

            ApplyPricing(partnerCar, partnerCar.CarModel.MarketValueKzt, partnerCar.CarModel.Rating);
            await _db.SaveChangesAsync(cancellationToken);
        }

        private static void ApplyPricing(PartnerCar partnerCar, decimal? marketValueKzt, decimal? modelRating)
        {
            var effectiveRating = partnerCar.Rating ?? modelRating;
            var (priceHour, priceDay) = PartnerCarDisplayPriceCalculator.Calculate(marketValueKzt, effectiveRating);

            partnerCar.PriceHour = priceHour;
            partnerCar.PriceDay = priceDay;
        }
    }
}
