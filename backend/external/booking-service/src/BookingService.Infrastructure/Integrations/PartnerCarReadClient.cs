using System.Net;
using System.Net.Http.Json;
using BookingService.Application.Interfaces.Integrations;

namespace BookingService.Infrastructure.Integrations
{
    public sealed class PartnerCarReadClient : IPartnerCarReadClient
    {
        private readonly HttpClient _httpClient;

        public PartnerCarReadClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PartnerCarSnapshot?> GetByIdAsync(int partnerCarId, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.GetAsync($"/partner-cars/{partnerCarId}", cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            var payload = await response.Content.ReadFromJsonAsync<PartnerCarPayload>(cancellationToken: cancellationToken);
            if (payload is null)
            {
                throw new InvalidOperationException("Car service returned empty response.");
            }

            return new PartnerCarSnapshot(payload.PartnerUserId, payload.PriceHour);
        }

        private sealed class PartnerCarPayload
        {
            public Guid PartnerUserId { get; init; }
            public decimal? PriceHour { get; init; }
        }
    }
}
