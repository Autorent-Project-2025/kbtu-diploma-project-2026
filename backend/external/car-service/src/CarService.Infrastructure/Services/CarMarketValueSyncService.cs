using CarService.Application.Interfaces;
using CarService.Application.Interfaces.Integrations;
using CarService.Domain.Constants;
using CarService.Domain.Entities;
using CarService.Infrastructure.Options;
using CarService.Infrastructure.Persistence;
using CarService.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CarService.Infrastructure.Services
{
    public sealed class CarMarketValueSyncService : ICarMarketValueSyncService
    {
        private readonly ApplicationDbContext _db;
        private readonly ICarMarketValueClient _carMarketValueClient;
        private readonly IPartnerCarDisplayPricingService _partnerCarDisplayPricingService;
        private readonly MarketValueRefreshOptions _options;
        private readonly ILogger<CarMarketValueSyncService> _logger;

        public CarMarketValueSyncService(
            ApplicationDbContext db,
            ICarMarketValueClient carMarketValueClient,
            IPartnerCarDisplayPricingService partnerCarDisplayPricingService,
            IOptions<MarketValueRefreshOptions> options,
            ILogger<CarMarketValueSyncService> logger)
        {
            _db = db;
            _carMarketValueClient = carMarketValueClient;
            _partnerCarDisplayPricingService = partnerCarDisplayPricingService;
            _options = options.Value;
            _logger = logger;
        }

        public async Task EnsureCarModelMarketValueAsync(
            int carModelId,
            CancellationToken cancellationToken = default)
        {
            var carModel = await _db.CarModels
                .AsNoTracking()
                .FirstOrDefaultAsync(entity => entity.Id == carModelId, cancellationToken);

            if (carModel is null)
            {
                return;
            }

            if (NeedsRefresh(carModel))
            {
                await RefreshCarModelMarketValueAsync(carModelId, cancellationToken);
                return;
            }

            await _partnerCarDisplayPricingService.RecalculateForCarModelAsync(carModelId, cancellationToken);
        }

        public async Task RefreshStaleCarModelsAsync(CancellationToken cancellationToken = default)
        {
            var cutoff = DateTimeOffset.UtcNow.AddHours(-_options.RefreshAfterHours);

            var staleCarModelIds = await _db.CarModels
                .AsNoTracking()
                .Where(entity =>
                    entity.MarketValueFetchedAt == null ||
                    entity.MarketValueFetchedAt <= cutoff ||
                    entity.MarketValueStatus == MarketValueStatusConstants.Pending)
                .OrderBy(entity => entity.MarketValueFetchedAt ?? DateTimeOffset.MinValue)
                .ThenBy(entity => entity.Id)
                .Take(_options.BatchSize)
                .Select(entity => entity.Id)
                .ToListAsync(cancellationToken);

            foreach (var carModelId in staleCarModelIds)
            {
                await RefreshCarModelMarketValueAsync(carModelId, cancellationToken);
            }
        }

        public async Task RefreshCarModelMarketValueAsync(
            int carModelId,
            CancellationToken cancellationToken = default)
        {
            var carModel = await _db.CarModels
                .IncludeCatalog()
                .FirstOrDefaultAsync(entity => entity.Id == carModelId, cancellationToken);

            if (carModel is null)
            {
                return;
            }

            try
            {
                var estimate = await _carMarketValueClient.GetMarketValueAsync(
                    carModel.Brand.Name,
                    carModel.ModelLookup.Name,
                    carModel.Year,
                    cancellationToken);

                carModel.MarketValueKzt = decimal.Round(estimate.MarketValueKzt, 2, MidpointRounding.AwayFromZero);
                carModel.MarketValueFetchedAt = estimate.FetchedAt == default
                    ? DateTimeOffset.UtcNow
                    : estimate.FetchedAt;
                carModel.MarketValueSource = NormalizeOptional(estimate.Source, 64);
                carModel.MarketValueSourceUrl = NormalizeOptional(estimate.SourceUrl, 2048);
                carModel.MarketValueSampleCount = Math.Max(0, estimate.SampleCount);
                carModel.MarketValueFilteredSampleCount = Math.Max(0, estimate.FilteredSampleCount);
                carModel.MarketValueConfidence = NormalizeOptional(estimate.Confidence, 16);
                carModel.MarketValueStatus = MarketValueStatusConstants.Success;
                carModel.MarketValueError = null;

                await _db.SaveChangesAsync(cancellationToken);

                try
                {
                    await _partnerCarDisplayPricingService.RecalculateForCarModelAsync(carModel.Id, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Market value was refreshed for car model {CarModelId}, but display price projection failed.",
                        carModel.Id);
                }
            }
            catch (Exception ex)
            {
                carModel.MarketValueFetchedAt = DateTimeOffset.UtcNow;
                carModel.MarketValueStatus = MarketValueStatusConstants.Failed;
                carModel.MarketValueError = Truncate(ex.Message, 4000);

                await _db.SaveChangesAsync(cancellationToken);

                _logger.LogWarning(
                    ex,
                    "Failed to refresh market value for car model {CarModelId} ({Brand} {Model} {Year}).",
                    carModel.Id,
                    carModel.Brand.Name,
                    carModel.ModelLookup.Name,
                    carModel.Year);
            }
        }

        private bool NeedsRefresh(Car carModel)
        {
            if (carModel.MarketValueKzt is null || carModel.MarketValueKzt <= 0m)
            {
                return true;
            }

            if (carModel.MarketValueFetchedAt is null)
            {
                return true;
            }

            if (string.Equals(carModel.MarketValueStatus, MarketValueStatusConstants.Pending, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var cutoff = DateTimeOffset.UtcNow.AddHours(-_options.RefreshAfterHours);
            return carModel.MarketValueFetchedAt <= cutoff;
        }

        private static string? NormalizeOptional(string? value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var normalized = value.Trim();
            if (normalized.Length <= maxLength)
            {
                return normalized;
            }

            return normalized[..maxLength];
        }

        private static string? Truncate(string? value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return value.Length <= maxLength ? value : value[..maxLength];
        }
    }
}
