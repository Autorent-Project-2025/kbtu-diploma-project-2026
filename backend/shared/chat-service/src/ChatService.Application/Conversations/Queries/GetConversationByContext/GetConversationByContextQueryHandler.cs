using ChatService.Application.Interfaces;
using ChatService.Application.Models;

namespace ChatService.Application.Conversations.Queries.GetConversationByContext;

public sealed class GetConversationByContextQueryHandler(IConversationRepository conversationRepo)
{
    public async Task<ConversationDto?> HandleAsync(GetConversationByContextQuery query, CancellationToken ct = default)
    {
        var conversation = await conversationRepo.GetByContextAsync(query.ContextType, query.ContextId, ct);
        if (conversation is null) return null;

        if (!conversation.HasActiveParticipant(query.UserId))
            throw new UnauthorizedAccessException("You are not a participant of this conversation.");

        return conversation.ToDto();
    }
}
