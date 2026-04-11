namespace ChatService.Application.Conversations.Queries.GetMessages;

public sealed record GetMessagesQuery(
    string ConversationId,
    string UserId,
    string? BeforeId,
    int Limit = 50);
