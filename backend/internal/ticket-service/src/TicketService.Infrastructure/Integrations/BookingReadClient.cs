using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TicketService.Application.Exceptions;
using TicketService.Application.Interfaces;
using TicketService.Application.Models;
using TicketService.Infrastructure.Options;

namespace TicketService.Infrastructure.Integrations;

public sealed class BookingReadClient : IBookingReadClient
{
    private const string InternalApiKeyHeader = "X-Internal-Api-Key";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly BookingServiceOptions _options;

    public BookingReadClient(
        HttpClient httpClient,
        IOptions<BookingServiceOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<BookingForComplaintResult?> GetBookingAsync(
        int bookingId,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/internal/bookings/{bookingId}");
        request.Headers.Add(InternalApiKeyHeader, _options.InternalApiKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"Booking service returned {(int)response.StatusCode} for booking {bookingId}: {errorBody}");
        }

        var body = await response.Content.ReadFromJsonAsync<BookingReadResponse>(JsonOptions, cancellationToken);
        if (body is null)
            return null;

        return new BookingForComplaintResult(
            body.Id,
            body.UserId,
            body.PartnerUserId,
            body.Status ?? string.Empty,
            body.CarBrand ?? string.Empty,
            body.CarModel ?? string.Empty,
            body.PartnerName,
            body.CoverImageUrl,
            body.StartTime,
            body.EndTime,
            body.TotalPrice,
            body.TripStartedAt);
    }

    private sealed class BookingReadResponse
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public Guid PartnerUserId { get; set; }
        public string? Status { get; set; }
        public string? CarBrand { get; set; }
        public string? CarModel { get; set; }
        public string? PartnerName { get; set; }
        public string? CoverImageUrl { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public decimal? TotalPrice { get; set; }
        public DateTimeOffset? TripStartedAt { get; set; }
    }
}
