namespace ChatService.Application.Conversations.Queries.GetConversationByContext;

public sealed record GetConversationByContextQuery(string ContextType, string ContextId, string UserId);
