using ChatService.Domain.Enums;

namespace ChatService.Domain.Entities;

public sealed class Conversation
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ContextType { get; set; } = string.Empty;
    public string ContextId { get; set; } = string.Empty;
    public string SourceService { get; set; } = string.Empty;
    public ConversationStatus Status { get; set; } = ConversationStatus.Open;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }
    public List<ConversationParticipant> Participants { get; set; } = [];

    public bool IsOpen => Status == ConversationStatus.Open;

    public ConversationParticipant? GetParticipant(string userId) =>
        Participants.FirstOrDefault(p => p.UserId == userId && p.IsActive);

    public bool HasActiveParticipant(string userId) =>
        Participants.Any(p => p.UserId == userId && p.IsActive);
}
