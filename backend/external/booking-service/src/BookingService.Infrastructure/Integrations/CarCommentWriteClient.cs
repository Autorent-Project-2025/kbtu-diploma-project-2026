using System.Net.Http.Json;
using System.Text.Json;
using BookingService.Application.Interfaces.Integrations;
using BookingService.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace BookingService.Infrastructure.Integrations;

public sealed class CarCommentWriteClient : ICarCommentWriteClient
{
    private const string InternalApiKeyHeader = "X-Internal-Api-Key";

    private readonly HttpClient _httpClient;
    private readonly CarServiceOptions _options;

    public CarCommentWriteClient(HttpClient httpClient, IOptions<CarServiceOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<CreatedCarCommentPayload> CreateForCompletedBookingAsync(
        CreateCompletedBookingCarCommentPayload payload,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(payload);

        if (string.IsNullOrWhiteSpace(_options.InternalApiKey))
        {
            throw new InvalidOperationException("CarService:InternalApiKey configuration is required.");
        }

        EnsureBaseUrlConfigured();

        using var request = new HttpRequestMessage(HttpMethod.Post, "/internal/comments");
        request.Headers.Add(InternalApiKeyHeader, _options.InternalApiKey);
        request.Content = JsonContent.Create(payload);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var createdComment = await response.Content.ReadFromJsonAsync<CreatedCarCommentPayload>(
                cancellationToken: cancellationToken);

            return createdComment
                ?? throw new InvalidOperationException("Car service returned empty comment response.");
        }

        var rawContent = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new InvalidOperationException(
            string.IsNullOrWhiteSpace(rawContent)
                ? $"Car service request failed with status code {(int)response.StatusCode}."
                : TryExtractErrorMessage(rawContent) ?? rawContent);
    }

    private void EnsureBaseUrlConfigured()
    {
        if (_httpClient.BaseAddress is null)
        {
            throw new InvalidOperationException("Configuration value 'CarService:BaseUrl' is required.");
        }
    }

    private static string? TryExtractErrorMessage(string rawContent)
    {
        try
        {
            using var document = JsonDocument.Parse(rawContent);
            if (document.RootElement.TryGetProperty("detail", out var detail))
            {
                return detail.GetString();
            }

            if (document.RootElement.TryGetProperty("error", out var error))
            {
                return error.GetString();
            }

            if (document.RootElement.TryGetProperty("message", out var message))
            {
                return message.GetString();
            }
        }
        catch
        {
            return null;
        }

        return null;
    }
}
