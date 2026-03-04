using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TicketService.Application.Exceptions;
using TicketService.Application.Interfaces;
using TicketService.Application.Models;

namespace TicketService.Infrastructure.Integrations;

public sealed class ImageStorageClient : IImageStorageClient
{
    private readonly HttpClient _httpClient;

    public ImageStorageClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ImageStorageUploadResult> UploadAsync(
        TicketDocumentFilePayload payload,
        string authorizationHeader,
        CancellationToken cancellationToken = default)
    {
        using var requestContent = new ByteArrayContent(payload.Content);
        requestContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(payload.ContentType);

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/images")
        {
            Content = requestContent
        };

        message.Headers.TryAddWithoutValidation("Authorization", authorizationHeader);

        using var response = await _httpClient.SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            await ThrowResponseExceptionAsync(response, cancellationToken);
        }

        var uploadResponse = await response.Content.ReadFromJsonAsync<ImageUploadResponse>(cancellationToken: cancellationToken);
        if (uploadResponse is null ||
            string.IsNullOrWhiteSpace(uploadResponse.ImageId) ||
            string.IsNullOrWhiteSpace(uploadResponse.ImageUrl))
        {
            throw new InvalidOperationException("Image service returned invalid upload response.");
        }

        return new ImageStorageUploadResult(uploadResponse.ImageId, uploadResponse.ImageUrl);
    }

    private static async Task ThrowResponseExceptionAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var errorMessage = await TryReadErrorMessageAsync(response, cancellationToken)
            ?? $"Image service request failed with status code {(int)response.StatusCode}.";

        throw response.StatusCode switch
        {
            HttpStatusCode.BadRequest => new ValidationException(errorMessage),
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => new UnauthorizedException(errorMessage),
            _ => new InvalidOperationException(errorMessage)
        };
    }

    private static async Task<string?> TryReadErrorMessageAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var rawContent = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(rawContent))
        {
            return null;
        }

        try
        {
            using var jsonDocument = JsonDocument.Parse(rawContent);
            if (jsonDocument.RootElement.TryGetProperty("error", out var errorProperty))
            {
                return errorProperty.GetString();
            }

            if (jsonDocument.RootElement.TryGetProperty("message", out var messageProperty))
            {
                return messageProperty.GetString();
            }
        }
        catch
        {
            return rawContent;
        }

        return rawContent;
    }

    private sealed class ImageUploadResponse
    {
        public string ImageId { get; init; } = string.Empty;
        public string ImageUrl { get; init; } = string.Empty;
    }
}
