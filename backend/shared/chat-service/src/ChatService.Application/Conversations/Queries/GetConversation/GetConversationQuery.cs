namespace ChatService.Application.Conversations.Queries.GetConversation;

public sealed record GetConversationQuery(string ConversationId, string UserId);
