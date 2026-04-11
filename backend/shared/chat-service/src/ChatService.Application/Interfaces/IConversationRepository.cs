using ChatService.Domain.Entities;

namespace ChatService.Application.Interfaces;

public interface IConversationRepository
{
    Task<Conversation?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<Conversation?> GetByContextAsync(string contextType, string contextId, CancellationToken ct = default);
    Task CreateAsync(Conversation conversation, CancellationToken ct = default);
    Task UpdateAsync(Conversation conversation, CancellationToken ct = default);
}
