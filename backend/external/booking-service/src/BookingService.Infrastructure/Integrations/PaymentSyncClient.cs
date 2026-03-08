using System.Net.Http.Json;
using BookingService.Application.Interfaces.Integrations;
using BookingService.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace BookingService.Infrastructure.Integrations
{
    public sealed class PaymentSyncClient : IPaymentSyncClient
    {
        private const string InternalApiKeyHeader = "X-Internal-Api-Key";

        private readonly HttpClient _httpClient;
        private readonly PaymentServiceOptions _options;

        public PaymentSyncClient(HttpClient httpClient, IOptions<PaymentServiceOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public Task RecordBookingConfirmedAsync(
            int bookingId,
            Guid userId,
            Guid partnerUserId,
            int partnerCarId,
            decimal? priceHour,
            decimal? totalPrice,
            CancellationToken cancellationToken = default)
        {
            return SendAsync(
                "/internal/payments/bookings/confirm",
                new
                {
                    bookingId,
                    userId,
                    partnerUserId,
                    partnerCarId,
                    priceHour,
                    totalPrice
                },
                cancellationToken);
        }

        public Task RecordBookingCanceledAsync(int bookingId, CancellationToken cancellationToken = default)
        {
            return SendAsync(
                "/internal/payments/bookings/cancel",
                new { bookingId },
                cancellationToken);
        }

        public Task RecordBookingCompletedAsync(int bookingId, CancellationToken cancellationToken = default)
        {
            return SendAsync(
                "/internal/payments/bookings/complete",
                new { bookingId },
                cancellationToken);
        }

        private async Task SendAsync(string path, object payload, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_options.InternalApiKey))
            {
                throw new InvalidOperationException("PaymentService:InternalApiKey configuration is required.");
            }

            using var message = new HttpRequestMessage(HttpMethod.Post, path)
            {
                Content = JsonContent.Create(payload)
            };

            message.Headers.Add(InternalApiKeyHeader, _options.InternalApiKey);

            using var response = await _httpClient.SendAsync(message, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            var rawContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(rawContent)
                    ? $"Payment service request failed with status code {(int)response.StatusCode}."
                    : rawContent);
        }
    }
}
