using ChatService.Application.Interfaces;
using ChatService.Application.Models;

namespace ChatService.Application.Conversations.Queries.GetMessages;

public sealed class GetMessagesQueryHandler(
    IConversationRepository conversationRepo,
    IMessageRepository messageRepo)
{
    public async Task<List<MessageDto>> HandleAsync(GetMessagesQuery query, CancellationToken ct = default)
    {
        var conversation = await conversationRepo.GetByIdAsync(query.ConversationId, ct)
            ?? throw new InvalidOperationException("Conversation not found.");

        var participant = conversation.GetParticipant(query.UserId)
            ?? throw new UnauthorizedAccessException("You are not a participant of this conversation.");

        if (!participant.CanRead)
            throw new UnauthorizedAccessException("You do not have read access.");

        var limit = Math.Clamp(query.Limit, 1, 100);
        var messages = await messageRepo.GetByConversationAsync(
            query.ConversationId, query.BeforeId, limit, ct);

        return messages
            .Where(m => m.IsVisibleTo(participant))
            .Select(m => m.ToDto())
            .ToList();
    }
}
