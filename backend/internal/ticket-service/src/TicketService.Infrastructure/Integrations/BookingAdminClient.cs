using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using TicketService.Application.Interfaces;
using TicketService.Infrastructure.Options;

namespace TicketService.Infrastructure.Integrations;

public sealed class BookingAdminClient : IBookingAdminClient
{
    private const string InternalApiKeyHeader = "X-Internal-Api-Key";

    private readonly HttpClient _httpClient;
    private readonly BookingServiceOptions _options;

    public BookingAdminClient(
        HttpClient httpClient,
        IOptions<BookingServiceOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<bool> CancelBookingAsync(int bookingId, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/internal/bookings/{bookingId}/cancel");
        request.Headers.Add(InternalApiKeyHeader, _options.InternalApiKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return false;

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"Booking service returned {(int)response.StatusCode} when canceling booking {bookingId}: {errorBody}");
        }

        return true;
    }

    public async Task<bool> ApprovePartnerCancellationAsync(
        int bookingId,
        Guid ticketId,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/internal/bookings/{bookingId}/partner-cancellation/approve");
        request.Headers.Add(InternalApiKeyHeader, _options.InternalApiKey);
        request.Content = JsonContent.Create(new { ticketId });

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return false;

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"Booking service returned {(int)response.StatusCode} when approving partner cancellation for booking {bookingId}: {errorBody}");
        }

        return true;
    }

    public async Task<bool> RejectPartnerCancellationAsync(
        int bookingId,
        Guid ticketId,
        string decisionReason,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/internal/bookings/{bookingId}/partner-cancellation/reject");
        request.Headers.Add(InternalApiKeyHeader, _options.InternalApiKey);
        request.Content = JsonContent.Create(new { ticketId, decisionReason });

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return false;

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"Booking service returned {(int)response.StatusCode} when rejecting partner cancellation for booking {bookingId}: {errorBody}");
        }

        return true;
    }
}
