using System.Net;
using System.Net.Http.Json;
using CarService.Application.Interfaces.Integrations;

namespace CarService.Infrastructure.Integrations
{
    public sealed class CarMarketValueClient : ICarMarketValueClient
    {
        private readonly HttpClient _httpClient;

        public CarMarketValueClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CarMarketValueEstimatePayload> GetMarketValueAsync(
            string brand,
            string model,
            int year,
            CancellationToken cancellationToken = default)
        {
            var requestUri =
                $"/market-value/estimate?brand={Uri.EscapeDataString(brand)}" +
                $"&model={Uri.EscapeDataString(model)}" +
                $"&year={year}";

            using var response = await _httpClient.GetAsync(requestUri, cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException(
                    $"Comparable listings were not found for {brand} {model} {year}.");
            }

            response.EnsureSuccessStatusCode();

            var payload = await response.Content.ReadFromJsonAsync<CarMarketValuePayload>(
                cancellationToken: cancellationToken);

            if (payload is null)
            {
                throw new InvalidOperationException("Car market value service returned empty response.");
            }

            return new CarMarketValueEstimatePayload
            {
                MarketValueKzt = payload.MarketValueKzt,
                SampleCount = payload.SampleCount,
                FilteredSampleCount = payload.FilteredSampleCount,
                Confidence = payload.Confidence ?? string.Empty,
                Source = payload.Source ?? string.Empty,
                SourceUrl = payload.SourceUrl ?? string.Empty,
                FetchedAt = payload.FetchedAt
            };
        }

        private sealed class CarMarketValuePayload
        {
            public decimal MarketValueKzt { get; init; }
            public int SampleCount { get; init; }
            public int FilteredSampleCount { get; init; }
            public string? Confidence { get; init; }
            public string? Source { get; init; }
            public string? SourceUrl { get; init; }
            public DateTimeOffset FetchedAt { get; init; }
        }
    }
}
