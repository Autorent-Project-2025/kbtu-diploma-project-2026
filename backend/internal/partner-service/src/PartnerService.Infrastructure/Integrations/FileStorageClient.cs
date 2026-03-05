using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PartnerService.Application.Interfaces;
using PartnerService.Application.Models;
using PartnerService.Infrastructure.Options;

namespace PartnerService.Infrastructure.Integrations;

public sealed class FileStorageClient : IFileStorageClient
{
    private const string InternalApiKeyHeader = "X-Internal-Api-Key";

    private readonly HttpClient _httpClient;
    private readonly FileServiceOptions _fileServiceOptions;

    public FileStorageClient(
        HttpClient httpClient,
        IOptions<FileServiceOptions> fileServiceOptions)
    {
        _httpClient = httpClient;
        _fileServiceOptions = fileServiceOptions.Value;
    }

    public async Task<FileTemporaryLinkResult> GetTemporaryLinkAsync(
        string fileName,
        int? ttlSeconds = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("fileName is required.", nameof(fileName));
        }

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/internal/files/temporary-link")
        {
            Content = JsonContent.Create(new
            {
                fileName = fileName.Trim(),
                ttlSeconds
            })
        };

        message.Headers.Add(InternalApiKeyHeader, _fileServiceOptions.InternalApiKey);

        using var response = await _httpClient.SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            await ThrowResponseExceptionAsync(response, cancellationToken);
        }

        var body = await response.Content.ReadFromJsonAsync<FileTemporaryLinkResponse>(cancellationToken: cancellationToken);
        if (body is null || string.IsNullOrWhiteSpace(body.FileName) || string.IsNullOrWhiteSpace(body.Url))
        {
            throw new InvalidOperationException("File service returned invalid temporary link response.");
        }

        if (!DateTime.TryParse(body.ExpiresAtUtc, out var expiresAtUtc))
        {
            expiresAtUtc = DateTime.UtcNow.AddMinutes(15);
        }

        return new FileTemporaryLinkResult(
            body.FileName,
            body.Url,
            DateTime.SpecifyKind(expiresAtUtc, DateTimeKind.Utc));
    }

    private static async Task ThrowResponseExceptionAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var errorMessage = await TryReadErrorMessageAsync(response, cancellationToken)
            ?? $"File service request failed with status code {(int)response.StatusCode}.";

        throw response.StatusCode switch
        {
            HttpStatusCode.BadRequest => new ArgumentException(errorMessage),
            HttpStatusCode.NotFound => new KeyNotFoundException(errorMessage),
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => new UnauthorizedAccessException(errorMessage),
            HttpStatusCode.Conflict => new InvalidOperationException(errorMessage),
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
            var root = jsonDocument.RootElement;

            if (root.TryGetProperty("error", out var errorProperty))
            {
                return errorProperty.GetString();
            }

            if (root.TryGetProperty("message", out var messageProperty))
            {
                return messageProperty.GetString();
            }

            if (root.TryGetProperty("detail", out var detailProperty))
            {
                return detailProperty.GetString();
            }

            if (root.TryGetProperty("title", out var titleProperty))
            {
                return titleProperty.GetString();
            }
        }
        catch
        {
            return rawContent;
        }

        return rawContent;
    }

    private sealed record FileTemporaryLinkResponse(
        string FileName,
        string Url,
        string ExpiresAtUtc);
}
