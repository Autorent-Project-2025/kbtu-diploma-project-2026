using System.Net;
using System.Net.Http.Json;
using BookingService.Application.Interfaces.Integrations;
using BookingService.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace BookingService.Infrastructure.Integrations
{
    public sealed class PartnerProfileReadClient : IPartnerProfileReadClient
    {
        private const string InternalApiKeyHeader = "X-Internal-Api-Key";

        private readonly HttpClient _httpClient;
        private readonly PartnerServiceOptions _options;

        public PartnerProfileReadClient(
            HttpClient httpClient,
            IOptions<PartnerServiceOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<PartnerPublicProfilePayload?> GetPublicProfileByRelatedUserIdAsync(
            Guid relatedUserId,
            CancellationToken cancellationToken = default)
        {
            if (relatedUserId == Guid.Empty)
            {
                throw new ArgumentException("Related user id is required.", nameof(relatedUserId));
            }

            if (string.IsNullOrWhiteSpace(_options.InternalApiKey))
            {
                throw new InvalidOperationException("PartnerService:InternalApiKey configuration is required.");
            }

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/internal/partners/public-profile/by-related-user/{Uri.EscapeDataString(relatedUserId.ToString())}");
            request.Headers.Add(InternalApiKeyHeader, _options.InternalApiKey);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            var payload = await response.Content.ReadFromJsonAsync<PartnerPublicProfilePayload>(cancellationToken: cancellationToken);
            if (payload is null)
            {
                throw new InvalidOperationException("Partner service returned empty response.");
            }

            return payload;
        }
    }
}
