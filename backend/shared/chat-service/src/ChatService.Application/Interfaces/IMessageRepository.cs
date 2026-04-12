using ChatService.Domain.Entities;

namespace ChatService.Application.Interfaces;

public interface IMessageRepository
{
    Task<Message?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<List<Message>> GetByConversationAsync(string conversationId, string? beforeId, int limit, CancellationToken ct = default);
    Task CreateAsync(Message message, CancellationToken ct = default);
    Task<long> CountAfterAsync(string conversationId, string messageId, CancellationToken ct = default);
    Task<Message?> GetByAttachmentIdAsync(string conversationId, string attachmentId, CancellationToken ct = default);
}
