using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PartnerService.Application.Interfaces;
using PartnerService.Infrastructure.Options;

namespace PartnerService.Infrastructure.Integrations;

public sealed class CarServiceClient : ICarServiceClient
{
    private const string InternalApiKeyHeader = "X-Internal-Api-Key";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly CarServiceOptions _options;

    public CarServiceClient(
        HttpClient httpClient,
        IOptions<CarServiceOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<int> SetPartnerCarsActiveAsync(
        Guid partnerUserId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/internal/partner-cars/by-partner/{partnerUserId}/set-active");
        request.Headers.Add(InternalApiKeyHeader, _options.InternalApiKey);
        request.Content = new StringContent(
            JsonSerializer.Serialize(new { isActive }, JsonOptions),
            Encoding.UTF8,
            "application/json");

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
            return 0;

        var result = await response.Content.ReadFromJsonAsync<SetActiveResult>(JsonOptions, cancellationToken);
        return result?.UpdatedCount ?? 0;
    }

    private sealed class SetActiveResult
    {
        public int UpdatedCount { get; set; }
    }
}
