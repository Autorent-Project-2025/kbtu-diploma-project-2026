using ChatService.Domain.Entities;
using MongoDB.Driver;

namespace ChatService.Infrastructure.Persistence;

public sealed class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(string connectionString, string databaseName)
    {
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
        EnsureIndexes();
    }

    public IMongoCollection<Conversation> Conversations =>
        _database.GetCollection<Conversation>("conversations");

    public IMongoCollection<Message> Messages =>
        _database.GetCollection<Message>("messages");

    private void EnsureIndexes()
    {
        Conversations.Indexes.CreateMany([
            new CreateIndexModel<Conversation>(
                Builders<Conversation>.IndexKeys
                    .Ascending(c => c.ContextType)
                    .Ascending(c => c.ContextId),
                new CreateIndexOptions { Unique = true }),
            new CreateIndexModel<Conversation>(
                Builders<Conversation>.IndexKeys.Ascending(c => c.Status))
        ]);

        Messages.Indexes.CreateMany([
            new CreateIndexModel<Message>(
                Builders<Message>.IndexKeys
                    .Ascending(m => m.ConversationId)
                    .Descending(m => m.CreatedAt)),
            new CreateIndexModel<Message>(
                Builders<Message>.IndexKeys.Ascending(m => m.SenderUserId))
        ]);
    }
}
