using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TicketService.Application.Interfaces;
using TicketService.Infrastructure.Options;

namespace TicketService.Infrastructure.Integrations;

public sealed class ChatServiceClient : IChatServiceClient
{
    private const string InternalApiKeyHeader = "X-Internal-Api-Key";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly ChatServiceOptions _options;
    private readonly ILogger<ChatServiceClient> _logger;

    public ChatServiceClient(
        HttpClient httpClient,
        IOptions<ChatServiceOptions> options,
        ILogger<ChatServiceClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string?> CreateConversationAsync(
        string contextType,
        string contextId,
        string sourceService,
        List<ChatParticipant> participants,
        string? systemMessage = null,
        CancellationToken ct = default)
    {
        try
        {
            var body = new
            {
                contextType,
                contextId,
                sourceService,
                participants = participants.Select(p => new
                {
                    p.UserId,
                    p.ActorType,
                    p.Role,
                    p.CanRead,
                    p.CanWrite,
                    p.CanSendInternal,
                    p.Email,
                    p.DisplayName
                }),
                systemMessage
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "/internal/conversations");
            request.Headers.Add(InternalApiKeyHeader, _options.InternalApiKey);
            request.Content = JsonContent.Create(body, options: JsonOptions);

            using var response = await _httpClient.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ConversationResponse>(JsonOptions, ct);
            return result?.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create conversation for {ContextType}/{ContextId}", contextType, contextId);
            return null;
        }
    }

    public async Task AddParticipantAsync(
        string conversationId,
        ChatParticipant participant,
        CancellationToken ct = default)
    {
        try
        {
            var body = new
            {
                participant.UserId,
                participant.ActorType,
                participant.Role,
                participant.CanRead,
                participant.CanWrite,
                participant.CanSendInternal,
                participant.Email,
                participant.DisplayName
            };

            using var request = new HttpRequestMessage(HttpMethod.Post,
                $"/internal/conversations/{conversationId}/participants");
            request.Headers.Add(InternalApiKeyHeader, _options.InternalApiKey);
            request.Content = JsonContent.Create(body, options: JsonOptions);

            using var response = await _httpClient.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add participant to conversation {ConversationId}", conversationId);
        }
    }

    public async Task CloseConversationAsync(
        string conversationId,
        string? systemMessage = null,
        CancellationToken ct = default)
    {
        try
        {
            var body = new { systemMessage };

            using var request = new HttpRequestMessage(HttpMethod.Post,
                $"/internal/conversations/{conversationId}/close");
            request.Headers.Add(InternalApiKeyHeader, _options.InternalApiKey);
            request.Content = JsonContent.Create(body, options: JsonOptions);

            using var response = await _httpClient.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to close conversation {ConversationId}", conversationId);
        }
    }

    public async Task ReopenConversationAsync(
        string conversationId,
        string? systemMessage = null,
        CancellationToken ct = default)
    {
        try
        {
            var body = new { systemMessage };

            using var request = new HttpRequestMessage(HttpMethod.Post,
                $"/internal/conversations/{conversationId}/reopen");
            request.Headers.Add(InternalApiKeyHeader, _options.InternalApiKey);
            request.Content = JsonContent.Create(body, options: JsonOptions);

            using var response = await _httpClient.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reopen conversation {ConversationId}", conversationId);
        }
    }

    public async Task SendSystemMessageAsync(
        string conversationId,
        string body,
        bool internalOnly = false,
        CancellationToken ct = default)
    {
        try
        {
            var payload = new { body, internalOnly };

            using var request = new HttpRequestMessage(HttpMethod.Post,
                $"/internal/conversations/{conversationId}/system-message");
            request.Headers.Add(InternalApiKeyHeader, _options.InternalApiKey);
            request.Content = JsonContent.Create(payload, options: JsonOptions);

            using var response = await _httpClient.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send system message to conversation {ConversationId}", conversationId);
        }
    }

    public async Task<string?> GetConversationIdByContextAsync(
        string contextType,
        string contextId,
        CancellationToken ct = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get,
                $"/internal/conversations/by-context/{contextType}/{contextId}");
            request.Headers.Add(InternalApiKeyHeader, _options.InternalApiKey);

            using var response = await _httpClient.SendAsync(request, ct);
            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ConversationResponse>(JsonOptions, ct);
            return result?.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get conversation for {ContextType}/{ContextId}", contextType, contextId);
            return null;
        }
    }

    private sealed class ConversationResponse
    {
        public string Id { get; set; } = string.Empty;
    }
}
