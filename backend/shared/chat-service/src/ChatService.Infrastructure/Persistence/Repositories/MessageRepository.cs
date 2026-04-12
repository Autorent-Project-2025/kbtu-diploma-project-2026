using ChatService.Application.Interfaces;
using ChatService.Domain.Entities;
using MongoDB.Driver;

namespace ChatService.Infrastructure.Persistence.Repositories;

public sealed class MessageRepository(MongoDbContext db) : IMessageRepository
{
    public async Task<Message?> GetByIdAsync(string id, CancellationToken ct = default) =>
        await db.Messages.Find(m => m.Id == id).FirstOrDefaultAsync(ct);

    public async Task<List<Message>> GetByConversationAsync(
        string conversationId, string? beforeId, int limit, CancellationToken ct = default)
    {
        var filter = Builders<Message>.Filter.Eq(m => m.ConversationId, conversationId);

        if (beforeId is not null)
        {
            var anchorMessage = await GetByIdAsync(beforeId, ct);
            if (anchorMessage is not null)
            {
                filter &= Builders<Message>.Filter.Lt(m => m.CreatedAt, anchorMessage.CreatedAt);
            }
        }

        return await db.Messages
            .Find(filter)
            .SortByDescending(m => m.CreatedAt)
            .Limit(limit)
            .ToListAsync(ct);
    }

    public async Task CreateAsync(Message message, CancellationToken ct = default) =>
        await db.Messages.InsertOneAsync(message, cancellationToken: ct);

    public async Task<Message?> GetByAttachmentIdAsync(string conversationId, string attachmentId, CancellationToken ct = default)
    {
        var filter = Builders<Message>.Filter.Eq(m => m.ConversationId, conversationId)
            & Builders<Message>.Filter.ElemMatch(m => m.Attachments, a => a.Id == attachmentId);
        return await db.Messages.Find(filter).FirstOrDefaultAsync(ct);
    }

    public async Task<long> CountAfterAsync(string conversationId, string messageId, CancellationToken ct = default)
    {
        var message = await GetByIdAsync(messageId, ct);
        if (message is null) return 0;

        return await db.Messages.CountDocumentsAsync(
            m => m.ConversationId == conversationId && m.CreatedAt > message.CreatedAt, cancellationToken: ct);
    }
}
