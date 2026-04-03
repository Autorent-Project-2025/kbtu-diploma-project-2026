using CarService.Application.Interfaces;
using CarService.Domain.Calculations;
using CarService.Domain.Entities;
using CarService.Infrastructure.Observability;
using CarService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CarService.Infrastructure.Services
{
    public sealed class PartnerCarDisplayPricingService : IPartnerCarDisplayPricingService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<PartnerCarDisplayPricingService> _logger;
        private readonly ObservabilityLogWriter _observabilityLogWriter;

        public PartnerCarDisplayPricingService(
            ApplicationDbContext db,
            ILogger<PartnerCarDisplayPricingService> logger,
            ObservabilityLogWriter observabilityLogWriter)
        {
            _db = db;
            _logger = logger;
            _observabilityLogWriter = observabilityLogWriter;
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

            var snapshots = new List<DisplayPriceSnapshot>(partnerCars.Count);
            foreach (var partnerCar in partnerCars)
            {
                snapshots.Add(ApplyPricing(partnerCar, carModel.MarketValueKzt, carModel.Rating));
            }

            await _db.SaveChangesAsync(cancellationToken);
            await WriteSnapshotsAsync(snapshots, cancellationToken);
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

            var snapshot = ApplyPricing(partnerCar, partnerCar.CarModel.MarketValueKzt, partnerCar.CarModel.Rating);
            await _db.SaveChangesAsync(cancellationToken);
            await WriteSnapshotsAsync([snapshot], cancellationToken);
        }

        private DisplayPriceSnapshot ApplyPricing(PartnerCar partnerCar, decimal? marketValueKzt, decimal? modelRating)
        {
            var effectiveRating = partnerCar.Rating ?? modelRating;
            var ratingCoefficient = 1m + ((effectiveRating ?? 3.0m) - 3.0m) * 0.05m;
            var (priceHour, priceDay) = PartnerCarDisplayPriceCalculator.Calculate(marketValueKzt, effectiveRating);

            partnerCar.PriceHour = priceHour;
            partnerCar.PriceDay = priceDay;

            return new DisplayPriceSnapshot(
                partnerCar.Id,
                partnerCar.CarModelId,
                marketValueKzt,
                effectiveRating ?? 3.0m,
                decimal.Round(ratingCoefficient, 4, MidpointRounding.AwayFromZero),
                priceHour,
                priceDay);
        }

        private async Task WriteSnapshotsAsync(
            IReadOnlyCollection<DisplayPriceSnapshot> snapshots,
            CancellationToken cancellationToken)
        {
            foreach (var snapshot in snapshots)
            {
                _logger.LogInformation(
                    "Display price recalculated for partner car {PartnerCarId}: carModelId={CarModelId}, marketValueKzt={MarketValueKzt}, effectiveRating={EffectiveRating}, ratingCoefficient={RatingCoefficient}, priceHour={PriceHour}, priceDay={PriceDay}.",
                    snapshot.PartnerCarId,
                    snapshot.CarModelId,
                    snapshot.MarketValueKzt,
                    snapshot.EffectiveRating,
                    snapshot.RatingCoefficient,
                    snapshot.PriceHour,
                    snapshot.PriceDay);

                await _observabilityLogWriter.WriteAsync(new
                {
                    timestamp = DateTimeOffset.UtcNow,
                    service = "car-service",
                    level = "Information",
                    @event = "partner_car_display_price_recalculated",
                    partnerCarId = snapshot.PartnerCarId,
                    carModelId = snapshot.CarModelId,
                    marketValueKzt = snapshot.MarketValueKzt,
                    effectiveRating = snapshot.EffectiveRating,
                    ratingCoefficient = snapshot.RatingCoefficient,
                    priceHour = snapshot.PriceHour,
                    priceDay = snapshot.PriceDay
                }, cancellationToken);
            }
        }

        private readonly record struct DisplayPriceSnapshot(
            int PartnerCarId,
            int CarModelId,
            decimal? MarketValueKzt,
            decimal EffectiveRating,
            decimal RatingCoefficient,
            decimal? PriceHour,
            decimal? PriceDay);
    }
}
