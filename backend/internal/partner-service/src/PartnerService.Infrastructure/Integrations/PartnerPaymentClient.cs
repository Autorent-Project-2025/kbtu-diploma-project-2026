using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PartnerService.Application.Interfaces;
using PartnerService.Application.Models;
using PartnerService.Infrastructure.Options;

namespace PartnerService.Infrastructure.Integrations;

public sealed class PartnerPaymentClient : IPartnerPaymentClient
{
    private const string InternalApiKeyHeader = "X-Internal-Api-Key";

    private readonly HttpClient _httpClient;
    private readonly PaymentServiceOptions _paymentServiceOptions;

    public PartnerPaymentClient(
        HttpClient httpClient,
        IOptions<PaymentServiceOptions> paymentServiceOptions)
    {
        _httpClient = httpClient;
        _paymentServiceOptions = paymentServiceOptions.Value;
    }

    public async Task<PartnerWalletResult> GetWalletAsync(Guid partnerUserId, CancellationToken cancellationToken = default)
    {
        using var message = CreateRequest(HttpMethod.Get, $"/internal/payments/wallets/{partnerUserId}");
        using var response = await _httpClient.SendAsync(message, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ThrowResponseExceptionAsync(response, cancellationToken);
        }

        return await ReadRequiredAsync<PartnerWalletResult>(response, "Partner wallet response is invalid.", cancellationToken);
    }

    public async Task<IReadOnlyCollection<PartnerLedgerEntryResult>> GetLedgerAsync(Guid partnerUserId, int take = 50, CancellationToken cancellationToken = default)
    {
        using var message = CreateRequest(HttpMethod.Get, $"/internal/payments/ledger/{partnerUserId}?take={Math.Clamp(take, 1, 200)}");
        using var response = await _httpClient.SendAsync(message, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ThrowResponseExceptionAsync(response, cancellationToken);
        }

        var result = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<PartnerLedgerEntryResult>>(cancellationToken: cancellationToken);
        return result ?? Array.Empty<PartnerLedgerEntryResult>();
    }

    public async Task<PartnerPayoutResult> RequestPayoutAsync(Guid partnerUserId, decimal amount, string requestKey, CancellationToken cancellationToken = default)
    {
        using var message = CreateRequest(HttpMethod.Post, "/internal/payments/payouts/request");
        message.Content = JsonContent.Create(new
        {
            partnerUserId,
            amount,
            requestKey
        });

        using var response = await _httpClient.SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            await ThrowResponseExceptionAsync(response, cancellationToken);
        }

        return await ReadRequiredAsync<PartnerPayoutResult>(response, "Partner payout response is invalid.", cancellationToken);
    }

    public async Task<PartnerPayoutResult?> GetPayoutAsync(long payoutId, CancellationToken cancellationToken = default)
    {
        using var message = CreateRequest(HttpMethod.Get, $"/internal/payments/payouts/{payoutId}");
        using var response = await _httpClient.SendAsync(message, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            await ThrowResponseExceptionAsync(response, cancellationToken);
        }

        return await response.Content.ReadFromJsonAsync<PartnerPayoutResult>(cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyCollection<PartnerPayoutResult>> GetPayoutsAsync(Guid partnerUserId, int take = 50, CancellationToken cancellationToken = default)
    {
        using var message = CreateRequest(HttpMethod.Get, $"/internal/payments/payouts/by-partner/{partnerUserId}?take={Math.Clamp(take, 1, 200)}");
        using var response = await _httpClient.SendAsync(message, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ThrowResponseExceptionAsync(response, cancellationToken);
        }

        var result = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<PartnerPayoutResult>>(cancellationToken: cancellationToken);
        return result ?? Array.Empty<PartnerPayoutResult>();
    }

    public async Task<PartnerPayoutResult> CancelPayoutAsync(long payoutId, string? reason = null, CancellationToken cancellationToken = default)
    {
        using var message = CreateRequest(HttpMethod.Post, $"/internal/payments/payouts/{payoutId}/cancel");
        message.Content = JsonContent.Create(new
        {
            reason
        });

        using var response = await _httpClient.SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            await ThrowResponseExceptionAsync(response, cancellationToken);
        }

        return await ReadRequiredAsync<PartnerPayoutResult>(response, "Partner payout response is invalid.", cancellationToken);
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string path)
    {
        if (string.IsNullOrWhiteSpace(_paymentServiceOptions.InternalApiKey))
        {
            throw new InvalidOperationException("PaymentService:InternalApiKey configuration is required.");
        }

        var message = new HttpRequestMessage(method, path);
        message.Headers.Add(InternalApiKeyHeader, _paymentServiceOptions.InternalApiKey);
        return message;
    }

    private static async Task<T> ReadRequiredAsync<T>(
        HttpResponseMessage response,
        string errorMessage,
        CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
        return body is null ? throw new InvalidOperationException(errorMessage) : body;
    }

    private static async Task ThrowResponseExceptionAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var errorMessage = await TryReadErrorMessageAsync(response, cancellationToken)
            ?? $"Payment service request failed with status code {(int)response.StatusCode}.";

        throw response.StatusCode switch
        {
            HttpStatusCode.BadRequest => new ArgumentException(errorMessage),
            HttpStatusCode.NotFound => new KeyNotFoundException(errorMessage),
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => new UnauthorizedAccessException(errorMessage),
            HttpStatusCode.Conflict => new InvalidOperationException(errorMessage),
            _ => new InvalidOperationException(errorMessage)
        };
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

            if (root.TryGetProperty("title", out var titleProperty))
            {
                return titleProperty.GetString();
            }
        }
        catch
        {
            return rawContent;
        }

        return rawContent;
    }
}
