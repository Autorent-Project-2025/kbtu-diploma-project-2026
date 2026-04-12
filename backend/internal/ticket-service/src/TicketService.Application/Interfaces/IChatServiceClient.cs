namespace TicketService.Application.Interfaces;

public interface IChatServiceClient
{
    Task<string?> CreateConversationAsync(
        string contextType,
        string contextId,
        string sourceService,
        List<ChatParticipant> participants,
        string? systemMessage = null,
        CancellationToken ct = default);

    Task AddParticipantAsync(
        string conversationId,
        ChatParticipant participant,
        CancellationToken ct = default);

    Task CloseConversationAsync(
        string conversationId,
        string? systemMessage = null,
        CancellationToken ct = default);

    Task ReopenConversationAsync(
        string conversationId,
        string? systemMessage = null,
        CancellationToken ct = default);

    Task SendSystemMessageAsync(
        string conversationId,
        string body,
        bool internalOnly = false,
        CancellationToken ct = default);

    Task<string?> GetConversationIdByContextAsync(
        string contextType,
        string contextId,
        CancellationToken ct = default);
}

public sealed record ChatParticipant(
    string UserId,
    string ActorType,
    string Role,
    bool CanRead = true,
    bool CanWrite = true,
    bool CanSendInternal = false,
    string? Email = null,
    string? DisplayName = null);
