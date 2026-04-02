using System.Net;
using System.Net.Http.Json;
using BookingService.Application.Interfaces.Integrations;
using BookingService.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace BookingService.Infrastructure.Integrations;

public sealed class IdentityUserReadClient : IIdentityUserReadClient
{
    private const string InternalApiKeyHeader = "X-Internal-Api-Key";

    private readonly HttpClient _httpClient;
    private readonly IdentityServiceOptions _options;

    public IdentityUserReadClient(HttpClient httpClient, IOptions<IdentityServiceOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<IdentityUserPayload?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id is required.", nameof(userId));
        }

        if (_httpClient.BaseAddress is null)
        {
            throw new InvalidOperationException("Configuration value 'IdentityService:BaseUrl' is required.");
        }

        if (string.IsNullOrWhiteSpace(_options.InternalApiKey))
        {
            throw new InvalidOperationException("IdentityService:InternalApiKey configuration is required.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, $"/internal/users/{userId}");
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
                    ? $"Identity service request failed with status code {(int)response.StatusCode}."
                    : rawContent);
        }

        return await response.Content.ReadFromJsonAsync<IdentityUserPayload>(cancellationToken: cancellationToken);
    }
}
