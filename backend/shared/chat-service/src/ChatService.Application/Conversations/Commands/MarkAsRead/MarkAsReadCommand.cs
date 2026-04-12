namespace ChatService.Application.Conversations.Commands.MarkAsRead;

public sealed record MarkAsReadCommand(
    string ConversationId,
    string UserId,
    string MessageId);
