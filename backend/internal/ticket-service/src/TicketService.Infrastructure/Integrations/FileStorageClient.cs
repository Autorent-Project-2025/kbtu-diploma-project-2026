using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TicketService.Application.Exceptions;
using TicketService.Application.Interfaces;
using TicketService.Application.Models;
using TicketService.Infrastructure.Options;

namespace TicketService.Infrastructure.Integrations;

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

    public async Task<string> UploadFileAsync(
        TicketDocumentFilePayload payload,
        CancellationToken cancellationToken = default)
    {
        using var content = new ByteArrayContent(payload.Content);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(payload.ContentType);

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/internal/files/upload")
        {
            Content = content
        };

        message.Headers.Add(InternalApiKeyHeader, _fileServiceOptions.InternalApiKey);
        message.Headers.Add("x-file-name", payload.FileName);

        using var response = await _httpClient.SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            await ThrowResponseExceptionAsync(response, cancellationToken);
        }

        var body = await response.Content.ReadFromJsonAsync<FileUploadResponse>(cancellationToken: cancellationToken);
        if (body is null || string.IsNullOrWhiteSpace(body.FileName))
        {
            throw new InvalidOperationException("File service returned invalid upload response.");
        }

        return body.FileName;
    }

    public async Task<FileTemporaryLinkResult> GetTemporaryLinkAsync(
        string fileName,
        int? ttlSeconds = null,
        CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/internal/files/temporary-link")
        {
            Content = JsonContent.Create(new
            {
                fileName,
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
            HttpStatusCode.BadRequest => new ValidationException(errorMessage),
            HttpStatusCode.Conflict => new ConflictException(errorMessage),
            HttpStatusCode.NotFound => new NotFoundException(errorMessage),
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

    private sealed record FileUploadResponse(string FileName);

    private sealed record FileTemporaryLinkResponse(
        string FileName,
        string Url,
        string ExpiresAtUtc);
}
