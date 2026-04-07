using System.Net;
using System.Net.Http.Json;
using BookingService.Application.Interfaces.Integrations;
using BookingService.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace BookingService.Infrastructure.Integrations
{
    public sealed class PartnerCarReadClient : IPartnerCarReadClient
    {
        private const string InternalApiKeyHeader = "X-Internal-Api-Key";

        private readonly HttpClient _httpClient;
        private readonly CarServiceOptions _options;

        public PartnerCarReadClient(
            HttpClient httpClient,
            IOptions<CarServiceOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<PartnerCarPricingContext?> GetPricingContextAsync(
            int partnerCarId,
            DateTimeOffset startTime,
            DateTimeOffset endTime,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_options.InternalApiKey))
            {
                throw new InvalidOperationException("CarService:InternalApiKey configuration is required.");
            }

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/internal/partner-cars/{partnerCarId}/pricing-context?startTime={Uri.EscapeDataString(startTime.ToString("O"))}&endTime={Uri.EscapeDataString(endTime.ToString("O"))}");
            request.Headers.Add(InternalApiKeyHeader, _options.InternalApiKey);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            var payload = await response.Content.ReadFromJsonAsync<PartnerCarPayload>(cancellationToken: cancellationToken);
            if (payload is null)
            {
                throw new InvalidOperationException("Car service returned empty response.");
            }

            return new PartnerCarPricingContext(
                payload.PartnerCarId,
                payload.PartnerUserId,
                payload.CarModelId,
                payload.MarketValueKzt,
                payload.Rating,
                payload.CurrentAvailableCarsCount,
                payload.IsMarketValueStale);
        }

        public async Task<PartnerCarSnapshotPayload?> GetSnapshotAsync(
            int partnerCarId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_options.InternalApiKey))
            {
                throw new InvalidOperationException("CarService:InternalApiKey configuration is required.");
            }

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/internal/partner-cars/{partnerCarId}/snapshot");
            request.Headers.Add(InternalApiKeyHeader, _options.InternalApiKey);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            var payload = await response.Content.ReadFromJsonAsync<PartnerCarSnapshotPayload>(
                cancellationToken: cancellationToken);
            if (payload is null)
            {
                throw new InvalidOperationException("Car service returned empty snapshot response.");
            }

            return payload;
        }

        private sealed class PartnerCarPayload
        {
            public int PartnerCarId { get; init; }
            public Guid PartnerUserId { get; init; }
            public int CarModelId { get; init; }
            public decimal? MarketValueKzt { get; init; }
            public decimal Rating { get; init; }
            public int CurrentAvailableCarsCount { get; init; }
            public bool IsMarketValueStale { get; init; }
        }
    }
}
