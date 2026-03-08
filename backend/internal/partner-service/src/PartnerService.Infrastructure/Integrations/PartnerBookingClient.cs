using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PartnerService.Application.Interfaces;
using PartnerService.Application.Models;
using PartnerService.Infrastructure.Options;

namespace PartnerService.Infrastructure.Integrations;

public sealed class PartnerBookingClient : IPartnerBookingClient
{
    private const string InternalApiKeyHeader = "X-Internal-Api-Key";

    private readonly HttpClient _httpClient;
    private readonly BookingServiceOptions _bookingServiceOptions;

    public PartnerBookingClient(
        HttpClient httpClient,
        IOptions<BookingServiceOptions> bookingServiceOptions)
    {
        _httpClient = httpClient;
        _bookingServiceOptions = bookingServiceOptions.Value;
    }

    public async Task<IReadOnlyCollection<PartnerBookingResult>> GetBookingsAsync(
        Guid partnerUserId,
        CancellationToken cancellationToken = default)
    {
        if (partnerUserId == Guid.Empty)
        {
            throw new ArgumentException("Partner user id is required.", nameof(partnerUserId));
        }

        using var message = CreateRequest(HttpMethod.Get, $"/internal/bookings/by-partner-user/{partnerUserId}");
        using var response = await _httpClient.SendAsync(message, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ThrowResponseExceptionAsync(response, cancellationToken);
        }

        var result = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<PartnerBookingResult>>(cancellationToken: cancellationToken);
        return result ?? Array.Empty<PartnerBookingResult>();
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string path)
    {
        if (string.IsNullOrWhiteSpace(_bookingServiceOptions.InternalApiKey))
        {
            throw new InvalidOperationException("BookingService:InternalApiKey configuration is required.");
        }

        var message = new HttpRequestMessage(method, path);
        message.Headers.Add(InternalApiKeyHeader, _bookingServiceOptions.InternalApiKey);
        return message;
    }

    private static async Task ThrowResponseExceptionAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var errorMessage = await TryReadErrorMessageAsync(response, cancellationToken)
            ?? $"Booking service request failed with status code {(int)response.StatusCode}.";

        throw new InvalidOperationException(errorMessage);
    }

    private static async Task<string?> TryReadErrorMessageAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var rawContent = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(rawContent))
        {
            return null;
        }

        try
        {
            using var jsonDocument = JsonDocument.Parse(rawContent);
            var root = jsonDocument.RootElement;

            if (root.TryGetProperty("error", out var errorProperty))
            {
                return errorProperty.GetString();
            }

            if (root.TryGetProperty("message", out var messageProperty))
            {
                return messageProperty.GetString();
            }

            if (root.TryGetProperty("detail", out var detailProperty))
            {
                return detailProperty.GetString();
            }
        }
        catch
        {
            return rawContent;
        }

        return rawContent;
    }
}
