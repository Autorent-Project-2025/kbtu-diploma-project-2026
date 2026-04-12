using System.Net;
using System.Net.Http.Json;
using CarService.Application.Interfaces.Integrations;
using CarService.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace CarService.Infrastructure.Integrations
{
    public sealed class ClientProfileReadClient : IClientProfileReadClient
    {
        private const string InternalApiKeyHeader = "X-Internal-Api-Key";

        private readonly HttpClient _httpClient;
        private readonly ClientServiceOptions _options;

        public ClientProfileReadClient(
            HttpClient httpClient,
            IOptions<ClientServiceOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<string?> GetAvatarUrlByRelatedUserIdAsync(
            string relatedUserId,
            CancellationToken cancellationToken = default)
        {
            var normalizedRelatedUserId = relatedUserId?.Trim();
            if (!Guid.TryParse(normalizedRelatedUserId, out var userGuid))
            {
                return null;
            }

            using var message = new HttpRequestMessage(
                HttpMethod.Get,
                $"/internal/clients/by-user/{userGuid:D}");
            message.Headers.Add(InternalApiKeyHeader, _options.InternalApiKey);

            using var response = await _httpClient.SendAsync(message, cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                var raw = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(
                    string.IsNullOrWhiteSpace(raw)
                        ? $"Client service request failed with status {(int)response.StatusCode}."
                        : raw);
            }

            var payload = await response.Content.ReadFromJsonAsync<ClientProfileResponseDto>(
                cancellationToken: cancellationToken);

            return string.IsNullOrWhiteSpace(payload?.AvatarUrl)
                ? null
                : payload.AvatarUrl.Trim();
        }

        private sealed class ClientProfileResponseDto
        {
            public string? AvatarUrl { get; init; }
        }
    }
}
