using ChatService.Domain.Enums;

namespace ChatService.Domain.Entities;

public sealed class ConversationParticipant
{
    public string UserId { get; set; } = string.Empty;
    public ActorType ActorType { get; set; }
    public ParticipantRole Role { get; set; }
    public bool CanRead { get; set; } = true;
    public bool CanWrite { get; set; } = true;
    public bool CanSendInternal { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LeftAt { get; set; }
    public string? LastReadMessageId { get; set; }
    public DateTime? LastReadAt { get; set; }

    /// <summary>
    /// Optional email for notifications. Set by the orchestrating service
    /// when creating the conversation. Not exposed in public DTOs.
    /// </summary>
    public string? Email { get; set; }
    public string? DisplayName { get; set; }

    public bool IsActive => LeftAt is null;

    public bool IsInternal =>
        ActorType is ActorType.Manager or ActorType.Supermanager or ActorType.Admin;
}
