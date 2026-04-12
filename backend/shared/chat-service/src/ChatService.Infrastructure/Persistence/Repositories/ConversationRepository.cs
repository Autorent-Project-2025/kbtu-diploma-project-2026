using ChatService.Application.Interfaces;
using ChatService.Domain.Entities;
using MongoDB.Driver;

namespace ChatService.Infrastructure.Persistence.Repositories;

public sealed class ConversationRepository(MongoDbContext db) : IConversationRepository
{
    public async Task<Conversation?> GetByIdAsync(string id, CancellationToken ct = default) =>
        await db.Conversations.Find(c => c.Id == id).FirstOrDefaultAsync(ct);

    public async Task<Conversation?> GetByContextAsync(string contextType, string contextId, CancellationToken ct = default) =>
        await db.Conversations
            .Find(c => c.ContextType == contextType && c.ContextId == contextId)
            .FirstOrDefaultAsync(ct);

    public async Task CreateAsync(Conversation conversation, CancellationToken ct = default) =>
        await db.Conversations.InsertOneAsync(conversation, cancellationToken: ct);

    public async Task UpdateAsync(Conversation conversation, CancellationToken ct = default) =>
        await db.Conversations.ReplaceOneAsync(c => c.Id == conversation.Id, conversation, cancellationToken: ct);
}
