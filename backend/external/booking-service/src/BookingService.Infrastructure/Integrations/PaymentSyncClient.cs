using System.Net.Http.Json;
using BookingService.Application.Interfaces.Integrations;
using BookingService.Infrastructure.Options;
using Microsoft.Extensions.Options;
using System.Net;

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

        public Task<MockPaymentAttemptPayload> StartMockPaymentAsync(
            int bookingId,
            Guid userId,
            decimal amount,
            string currency,
            CancellationToken cancellationToken = default)
        {
            return SendForResponseAsync<MockPaymentAttemptPayload>(
                HttpMethod.Post,
                "/internal/mock-payments/start",
                new
                {
                    bookingId,
                    userId,
                    amount,
                    currency
                },
                cancellationToken);
        }

        public Task<MockPaymentAttemptPayload?> GetLatestMockPaymentAsync(
            int bookingId,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return SendForOptionalResponseAsync<MockPaymentAttemptPayload>(
                HttpMethod.Get,
                $"/internal/mock-payments/by-booking/{bookingId}?userId={userId}",
                null,
                cancellationToken);
        }

        public Task<MockPaymentAttemptPayload> SubmitMockPaymentAsync(
            int bookingId,
            Guid userId,
            string sessionKey,
            string cardHolder,
            string cardNumber,
            int expiryMonth,
            int expiryYear,
            string cvv,
            CancellationToken cancellationToken = default)
        {
            return SendForResponseAsync<MockPaymentAttemptPayload>(
                HttpMethod.Post,
                $"/internal/mock-payments/{bookingId}/submit",
                new
                {
                    userId,
                    sessionKey,
                    cardHolder,
                    cardNumber,
                    expiryMonth,
                    expiryYear,
                    cvv
                },
                cancellationToken);
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

        private async Task<T> SendForResponseAsync<T>(
            HttpMethod method,
            string path,
            object? payload,
            CancellationToken cancellationToken)
        {
            using var response = await SendRequestAsync(method, path, payload, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                await ThrowResponseExceptionAsync(response, cancellationToken);
            }

            var body = await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
            return body is null
                ? throw new InvalidOperationException("Payment service response body is invalid.")
                : body;
        }

        private async Task<T?> SendForOptionalResponseAsync<T>(
            HttpMethod method,
            string path,
            object? payload,
            CancellationToken cancellationToken)
        {
            using var response = await SendRequestAsync(method, path, payload, cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return default;
            }

            if (!response.IsSuccessStatusCode)
            {
                await ThrowResponseExceptionAsync(response, cancellationToken);
            }

            return await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
        }

        private async Task SendAsync(string path, object payload, CancellationToken cancellationToken)
        {
            using var response = await SendRequestAsync(HttpMethod.Post, path, payload, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            await ThrowResponseExceptionAsync(response, cancellationToken);
        }

        private async Task<HttpResponseMessage> SendRequestAsync(
            HttpMethod method,
            string path,
            object? payload,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_options.InternalApiKey))
            {
                throw new InvalidOperationException("PaymentService:InternalApiKey configuration is required.");
            }

            var message = new HttpRequestMessage(method, path);
            if (payload is not null)
            {
                message.Content = JsonContent.Create(payload);
            }

            message.Headers.Add(InternalApiKeyHeader, _options.InternalApiKey);

            return await _httpClient.SendAsync(message, cancellationToken);
        }

        private static async Task ThrowResponseExceptionAsync(
            HttpResponseMessage response,
            CancellationToken cancellationToken)
        {
            var rawContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(rawContent)
                    ? $"Payment service request failed with status code {(int)response.StatusCode}."
                    : rawContent);
        }
    }
}
