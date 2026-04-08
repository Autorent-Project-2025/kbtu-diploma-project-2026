using System.Net;
using ClientService.Application.Interfaces.Integrations;

namespace ClientService.Infrastructure.Integrations;

public sealed class ImageStorageClient : IImageStorageClient
{
    private readonly HttpClient _httpClient;

    public ImageStorageClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task DeleteAsync(
        string imageId,
        string authorizationHeader,
        CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(
            HttpMethod.Delete,
            $"/api/images/{Uri.EscapeDataString(imageId)}");
        message.Headers.TryAddWithoutValidation("Authorization", authorizationHeader);

        using var response = await _httpClient.SendAsync(message, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return;
        }

        if (!response.IsSuccessStatusCode)
        {
            var raw = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(raw)
                    ? $"Image service delete failed with status {(int)response.StatusCode}."
                    : raw);
        }
    }
}
