namespace ChatService.Application.Models;

public sealed record ConversationDto(
    string Id,
    string ContextType,
    string ContextId,
    string SourceService,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? ClosedAt,
    List<ParticipantDto> Participants);

public sealed record ParticipantDto(
    string UserId,
    string ActorType,
    string Role,
    bool CanRead,
    bool CanWrite,
    bool CanSendInternal,
    DateTime JoinedAt,
    DateTime? LeftAt,
    string? LastReadMessageId,
    DateTime? LastReadAt);
