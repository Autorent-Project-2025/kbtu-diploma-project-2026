using ChatService.Domain.Enums;

namespace ChatService.Domain.Entities;

public sealed class Message
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ConversationId { get; set; } = string.Empty;
    public string SenderUserId { get; set; } = string.Empty;
    public ActorType SenderActorType { get; set; }
    public MessageType MessageType { get; set; } = MessageType.Text;
    public MessageVisibility Visibility { get; set; } = MessageVisibility.Participants;
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<MessageAttachment> Attachments { get; set; } = [];

    public bool IsVisibleTo(ConversationParticipant participant)
    {
        if (!participant.CanRead) return false;
        if (Visibility == MessageVisibility.Participants) return true;
        return participant.IsInternal;
    }
}
