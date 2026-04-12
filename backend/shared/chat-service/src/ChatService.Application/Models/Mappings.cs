using ChatService.Domain.Entities;

namespace ChatService.Application.Models;

public static class Mappings
{
    public static ConversationDto ToDto(this Conversation c) => new(
        c.Id,
        c.ContextType,
        c.ContextId,
        c.SourceService,
        c.Status.ToString(),
        c.CreatedAt,
        c.UpdatedAt,
        c.ClosedAt,
        c.Participants.Select(p => p.ToDto()).ToList());

    public static ParticipantDto ToDto(this ConversationParticipant p) => new(
        p.UserId,
        p.ActorType.ToString(),
        p.Role.ToString(),
        p.CanRead,
        p.CanWrite,
        p.CanSendInternal,
        p.JoinedAt,
        p.LeftAt,
        p.LastReadMessageId,
        p.LastReadAt);

    public static MessageDto ToDto(this Message m) => new(
        m.Id,
        m.ConversationId,
        m.SenderUserId,
        m.SenderActorType.ToString(),
        m.MessageType.ToString(),
        m.Visibility.ToString(),
        m.Body,
        m.CreatedAt,
        m.Attachments.Select(a => a.ToDto()).ToList());

    public static AttachmentDto ToDto(this MessageAttachment a) => new(
        a.Id,
        a.FileName,
        a.OriginalFileName,
        a.MimeType,
        a.CreatedAt);
}
