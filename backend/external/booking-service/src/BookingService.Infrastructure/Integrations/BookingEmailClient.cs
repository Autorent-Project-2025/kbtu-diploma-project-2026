using System.Net.Http.Json;
using BookingService.Application.Interfaces.Integrations;

namespace BookingService.Infrastructure.Integrations;

public sealed class BookingEmailClient : IBookingEmailClient
{
    private readonly HttpClient _httpClient;

    public BookingEmailClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task SendCustomEmailAsync(
        string to,
        string subject,
        string text,
        string? html = null,
        CancellationToken cancellationToken = default)
    {
        if (_httpClient.BaseAddress is null)
        {
            throw new InvalidOperationException("Configuration value 'EmailService:BaseUrl' is required.");
        }

        using var response = await _httpClient.PostAsJsonAsync(
            "/emails/custom",
            new
            {
                to,
                subject,
                text,
                html
            },
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var rawContent = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new InvalidOperationException(
            string.IsNullOrWhiteSpace(rawContent)
                ? $"Email service request failed with status code {(int)response.StatusCode}."
                : rawContent);
    }
}
