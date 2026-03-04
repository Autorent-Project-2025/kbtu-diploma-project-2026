using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CarService.Application.Interfaces.Integrations;

namespace CarService.Infrastructure.Integrations
{
    public sealed class PartnerContextClient : IPartnerContextClient
    {
        private readonly HttpClient _httpClient;

        public PartnerContextClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PartnerContextDto?> GetCurrentPartnerAsync(
            string authorizationHeader,
            CancellationToken cancellationToken = default)
        {
            using var message = new HttpRequestMessage(HttpMethod.Get, "/me");
            message.Headers.TryAddWithoutValidation("Authorization", authorizationHeader);

            using var response = await _httpClient.SendAsync(message, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                throw new UnauthorizedAccessException("Partner service rejected the current user token.");
            }

            if (!response.IsSuccessStatusCode)
            {
                var raw = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(
                    string.IsNullOrWhiteSpace(raw)
                        ? $"Partner service request failed with status {(int)response.StatusCode}."
                        : raw);
            }

            var payload = await response.Content.ReadFromJsonAsync<PartnerContextDto>(cancellationToken: cancellationToken);
            return payload;
        }
    }
}
