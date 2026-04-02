using System.Net;
using System.Net.Http.Json;
using BookingService.Application.Interfaces.Integrations;
using BookingService.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace BookingService.Infrastructure.Integrations;

public sealed class ClientBookingAccessClient : IClientBookingAccessClient
{
    private const string InternalApiKeyHeader = "X-Internal-Api-Key";

    private readonly HttpClient _httpClient;
    private readonly ClientServiceOptions _options;

    public ClientBookingAccessClient(HttpClient httpClient, IOptions<ClientServiceOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<ClientBookingAccessPayload?> GetBookingAccessAsync(
        Guid relatedUserId,
        CancellationToken cancellationToken = default)
    {
        if (relatedUserId == Guid.Empty)
        {
            throw new ArgumentException("Related user id is required.", nameof(relatedUserId));
        }

        if (string.IsNullOrWhiteSpace(_options.InternalApiKey))
        {
            throw new InvalidOperationException("ClientService:InternalApiKey configuration is required.");
        }

        EnsureBaseUrlConfigured();

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/internal/clients/by-user/{relatedUserId}/booking-access");
        request.Headers.Add(InternalApiKeyHeader, _options.InternalApiKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            var rawContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(rawContent)
                    ? $"Client service request failed with status code {(int)response.StatusCode}."
                    : rawContent);
        }

        return await response.Content.ReadFromJsonAsync<ClientBookingAccessPayload>(cancellationToken: cancellationToken);
    }

    public async Task<ClientProfilePayload?> GetClientProfileAsync(
        Guid relatedUserId,
        CancellationToken cancellationToken = default)
    {
        if (relatedUserId == Guid.Empty)
        {
            throw new ArgumentException("Related user id is required.", nameof(relatedUserId));
        }

        if (string.IsNullOrWhiteSpace(_options.InternalApiKey))
        {
            throw new InvalidOperationException("ClientService:InternalApiKey configuration is required.");
        }

        EnsureBaseUrlConfigured();

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/internal/clients/by-user/{relatedUserId}");
        request.Headers.Add(InternalApiKeyHeader, _options.InternalApiKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            var rawContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(rawContent)
                    ? $"Client service request failed with status code {(int)response.StatusCode}."
                    : rawContent);
        }

        return await response.Content.ReadFromJsonAsync<ClientProfilePayload>(cancellationToken: cancellationToken);
    }

    public async Task<ClientProfilePayload?> SetBookingActionsBlockedAsync(
        Guid relatedUserId,
        bool isBlocked,
        string? reason,
        CancellationToken cancellationToken = default)
    {
        if (relatedUserId == Guid.Empty)
        {
            throw new ArgumentException("Related user id is required.", nameof(relatedUserId));
        }

        if (string.IsNullOrWhiteSpace(_options.InternalApiKey))
        {
            throw new InvalidOperationException("ClientService:InternalApiKey configuration is required.");
        }

        EnsureBaseUrlConfigured();

        var path = isBlocked
            ? $"/internal/clients/by-user/{relatedUserId}/booking-access/block"
            : $"/internal/clients/by-user/{relatedUserId}/booking-access/unblock";

        using var request = new HttpRequestMessage(HttpMethod.Post, path);
        request.Headers.Add(InternalApiKeyHeader, _options.InternalApiKey);
        if (isBlocked)
        {
            request.Content = JsonContent.Create(new { reason });
        }

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            var rawContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(rawContent)
                    ? $"Client service request failed with status code {(int)response.StatusCode}."
                    : rawContent);
        }

        return await response.Content.ReadFromJsonAsync<ClientProfilePayload>(cancellationToken: cancellationToken);
    }

    private void EnsureBaseUrlConfigured()
    {
        if (_httpClient.BaseAddress is null)
        {
            throw new InvalidOperationException("Configuration value 'ClientService:BaseUrl' is required.");
        }
    }
}
