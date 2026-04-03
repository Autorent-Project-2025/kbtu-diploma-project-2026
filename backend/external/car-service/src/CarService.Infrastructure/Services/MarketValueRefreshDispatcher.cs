using CarService.Application.Interfaces;
using CarService.Infrastructure.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CarService.Infrastructure.Services
{
    public sealed class MarketValueRefreshDispatcher : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MarketValueRefreshDispatcher> _logger;
        private readonly MarketValueRefreshOptions _options;

        public MarketValueRefreshDispatcher(
            IServiceScopeFactory scopeFactory,
            IOptions<MarketValueRefreshOptions> options,
            ILogger<MarketValueRefreshDispatcher> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_options.PollIntervalSeconds));

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await RefreshMarketValuesAsync(stoppingToken);
                    await timer.WaitForNextTickAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }
        }

        private async Task RefreshMarketValuesAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var syncService = scope.ServiceProvider.GetRequiredService<ICarMarketValueSyncService>();

            try
            {
                await syncService.RefreshStaleCarModelsAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Market value refresh cycle failed.");
            }
        }
    }
}
