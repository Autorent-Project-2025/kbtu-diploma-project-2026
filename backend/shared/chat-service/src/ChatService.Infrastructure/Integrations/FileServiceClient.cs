using System.Net.Http.Json;
using System.Text.Json;
using ChatService.Application.Interfaces;
using ChatService.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChatService.Infrastructure.Integrations;

public sealed class FileServiceClient : IFileServiceClient
{
    private const string InternalApiKeyHeader = "x-internal-api-key";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly FileServiceOptions _options;
    private readonly ILogger<FileServiceClient> _logger;

    public FileServiceClient(
        HttpClient httpClient,
        IOptions<FileServiceOptions> options,
        ILogger<FileServiceClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<FileUploadResult> UploadFileAsync(
        Stream fileStream, string fileName, string contentType, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/internal/files/upload");
        request.Headers.Add(InternalApiKeyHeader, _options.InternalApiKey);
        request.Headers.Add("x-file-name", fileName);

        var content = new StreamContent(fileStream);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        request.Content = content;

        using var response = await _httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<UploadResponse>(JsonOptions, ct);
        return new FileUploadResult(
            result?.FileName ?? throw new InvalidOperationException("File service returned no fileName"),
            fileName);
    }

    public async Task<string> GetTemporaryLinkAsync(
        string storedFileName, int ttlSeconds = 900, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/internal/files/temporary-link");
        request.Headers.Add(InternalApiKeyHeader, _options.InternalApiKey);
        request.Content = JsonContent.Create(new { fileName = storedFileName, ttlSeconds }, options: JsonOptions);

        using var response = await _httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<TemporaryLinkResponse>(JsonOptions, ct);
        return result?.Url ?? throw new InvalidOperationException("File service returned no URL");
    }

    private sealed class UploadResponse
    {
        public string FileName { get; set; } = string.Empty;
    }

    private sealed class TemporaryLinkResponse
    {
        public string Url { get; set; } = string.Empty;
    }
}
