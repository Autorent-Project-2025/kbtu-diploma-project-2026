using System.Net;
using System.Net.Http.Json;
using CarService.Application.DTOs.Bookings;
using CarService.Application.Interfaces.Integrations;
using Microsoft.Extensions.Options;
using CarService.Infrastructure.Options;

namespace CarService.Infrastructure.Integrations
{
    public sealed class BookingReadClient : IBookingReadClient
    {
        private const string InternalApiKeyHeader = "X-Internal-Api-Key";

        private readonly HttpClient _httpClient;
        private readonly BookingServiceOptions _options;

        public BookingReadClient(
            HttpClient httpClient,
            IOptions<BookingServiceOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<IReadOnlyCollection<CarBookingCountDto>> GetBookingCountsByCarIdsAsync(
            IReadOnlyCollection<int> carIds,
            CancellationToken cancellationToken = default)
        {
            if (carIds.Count == 0)
            {
                return [];
            }

            var query = string.Join(",", carIds.Distinct().OrderBy(id => id));

            using var message = new HttpRequestMessage(HttpMethod.Get, $"/internal/bookings/counts?carIds={Uri.EscapeDataString(query)}");
            message.Headers.Add(InternalApiKeyHeader, _options.InternalApiKey);

            using var response = await _httpClient.SendAsync(message, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var raw = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(
                    string.IsNullOrWhiteSpace(raw)
                        ? $"Booking service request failed with status {(int)response.StatusCode}."
                        : raw);
            }

            var payload = await response.Content.ReadFromJsonAsync<List<CarBookingCountDto>>(cancellationToken: cancellationToken);
            return payload ?? [];
        }

        public async Task<IReadOnlyCollection<LinkedBookingDto>> GetBookingsByCarIdAsync(
            int carId,
            CancellationToken cancellationToken = default)
        {
            using var message = new HttpRequestMessage(HttpMethod.Get, $"/internal/bookings/by-car/{carId}");
            message.Headers.Add(InternalApiKeyHeader, _options.InternalApiKey);

            using var response = await _httpClient.SendAsync(message, cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return [];
            }

            if (!response.IsSuccessStatusCode)
            {
                var raw = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(
                    string.IsNullOrWhiteSpace(raw)
                        ? $"Booking service request failed with status {(int)response.StatusCode}."
                        : raw);
            }

            var payload = await response.Content.ReadFromJsonAsync<List<LinkedBookingDto>>(cancellationToken: cancellationToken);
            return payload ?? [];
        }
    }
}
