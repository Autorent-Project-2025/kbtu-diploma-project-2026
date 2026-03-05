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

            using var message = new HttpRequestMessage(HttpMethod.Get, $"/internal/bookings/counts?partnerCarIds={Uri.EscapeDataString(query)}");
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

            var payload = await response.Content.ReadFromJsonAsync<List<BookingCountResponseDto>>(cancellationToken: cancellationToken) ?? [];
            return payload
                .Select(item => new CarBookingCountDto
                {
                    CarId = item.PartnerCarId,
                    Count = item.Count
                })
                .ToList();
        }

        public async Task<IReadOnlyCollection<LinkedBookingDto>> GetBookingsByCarIdAsync(
            int carId,
            CancellationToken cancellationToken = default)
        {
            using var message = new HttpRequestMessage(HttpMethod.Get, $"/internal/bookings/by-partner-car/{carId}");
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

            var payload = await response.Content.ReadFromJsonAsync<List<LinkedBookingResponseDto>>(cancellationToken: cancellationToken) ?? [];
            return payload
                .Select(item => new LinkedBookingDto
                {
                    Id = item.Id,
                    CarId = item.PartnerCarId,
                    UserId = item.UserId,
                    StartDate = item.StartTime.UtcDateTime,
                    EndDate = item.EndTime.UtcDateTime,
                    Price = item.TotalPrice,
                    Status = item.Status
                })
                .ToList();
        }

        public async Task<IReadOnlyCollection<CarAvailabilityDto>> CheckAvailabilityByCarIdsAsync(
            IReadOnlyCollection<int> carIds,
            DateTimeOffset startTime,
            DateTimeOffset endTime,
            CancellationToken cancellationToken = default)
        {
            var normalizedIds = carIds
                .Where(id => id > 0)
                .Distinct()
                .ToArray();

            if (normalizedIds.Length == 0)
            {
                return [];
            }

            using var message = new HttpRequestMessage(HttpMethod.Post, "/internal/bookings/check-availability");
            message.Headers.Add(InternalApiKeyHeader, _options.InternalApiKey);
            message.Content = JsonContent.Create(new
            {
                carIds = normalizedIds,
                startTime,
                endTime
            });

            using var response = await _httpClient.SendAsync(message, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var raw = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(
                    string.IsNullOrWhiteSpace(raw)
                        ? $"Booking service request failed with status {(int)response.StatusCode}."
                        : raw);
            }

            var payload = await response.Content.ReadFromJsonAsync<List<AvailabilityResponseDto>>(cancellationToken: cancellationToken) ?? [];
            return payload
                .Select(item => new CarAvailabilityDto
                {
                    CarId = item.PartnerCarId,
                    IsAvailable = item.IsAvailable,
                    NextAvailableFrom = item.NextAvailableFrom
                })
                .ToList();
        }

        private sealed class BookingCountResponseDto
        {
            public int PartnerCarId { get; init; }
            public int Count { get; init; }
        }

        private sealed class LinkedBookingResponseDto
        {
            public int Id { get; init; }
            public int PartnerCarId { get; init; }
            public string UserId { get; init; } = string.Empty;
            public DateTimeOffset StartTime { get; init; }
            public DateTimeOffset EndTime { get; init; }
            public decimal? TotalPrice { get; init; }
            public string? Status { get; init; }
        }

        private sealed class AvailabilityResponseDto
        {
            public int PartnerCarId { get; init; }
            public bool IsAvailable { get; init; }
            public DateTimeOffset NextAvailableFrom { get; init; }
        }
    }
}
