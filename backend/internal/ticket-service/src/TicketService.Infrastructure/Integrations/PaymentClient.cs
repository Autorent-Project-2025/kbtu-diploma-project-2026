using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TicketService.Application.Interfaces;
using TicketService.Infrastructure.Options;

namespace TicketService.Infrastructure.Integrations;

public sealed class PaymentClient : IPaymentClient
{
    private const string InternalApiKeyHeader = "X-Internal-Api-Key";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly PaymentServiceOptions _options;

    public PaymentClient(
        HttpClient httpClient,
        IOptions<PaymentServiceOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<bool> CancelBookingChargeAsync(
        long chargeId,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/internal/payments/booking-charges/{chargeId}/cancel");
        request.Headers.Add(InternalApiKeyHeader, _options.InternalApiKey);
        request.Content = new StringContent(
            JsonSerializer.Serialize(new { reason }, JsonOptions),
            Encoding.UTF8,
            "application/json");

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return false;

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"Payment service returned {(int)response.StatusCode} when canceling charge {chargeId}: {errorBody}");
        }

        return true;
    }

    public async Task<bool> RefundBookingChargeAsync(
        long chargeId,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/internal/payments/booking-charges/{chargeId}/refund");
        request.Headers.Add(InternalApiKeyHeader, _options.InternalApiKey);
        request.Content = new StringContent(
            JsonSerializer.Serialize(new { reason }, JsonOptions),
            Encoding.UTF8,
            "application/json");

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return false;

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"Payment service returned {(int)response.StatusCode} when refunding charge {chargeId}: {errorBody}");
        }

        return true;
    }

    public async Task<IReadOnlyCollection<BookingChargeInfo>> GetBookingChargesAsync(
        int bookingId,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/internal/payments/bookings/{bookingId}/charges");
        request.Headers.Add(InternalApiKeyHeader, _options.InternalApiKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
            return [];

        var charges = await response.Content.ReadFromJsonAsync<List<ChargeResponse>>(JsonOptions, cancellationToken);
        if (charges is null)
            return [];

        return charges.Select(c => new BookingChargeInfo(
            c.Id, c.BookingId, c.ChargeType ?? string.Empty, c.Amount,
            c.Status ?? string.Empty, c.Description, c.RefundedAt)).ToArray();
    }

    private sealed class ChargeResponse
    {
        public long Id { get; set; }
        public int BookingId { get; set; }
        public string? ChargeType { get; set; }
        public decimal Amount { get; set; }
        public string? Status { get; set; }
        public string? Description { get; set; }
        public DateTimeOffset? RefundedAt { get; set; }
    }
}
