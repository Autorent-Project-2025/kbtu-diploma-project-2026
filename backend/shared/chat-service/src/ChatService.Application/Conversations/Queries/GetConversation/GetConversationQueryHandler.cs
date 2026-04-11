using ChatService.Application.Interfaces;
using ChatService.Application.Models;

namespace ChatService.Application.Conversations.Queries.GetConversation;

public sealed class GetConversationQueryHandler(IConversationRepository conversationRepo)
{
    public async Task<ConversationDto> HandleAsync(GetConversationQuery query, CancellationToken ct = default)
    {
        var conversation = await conversationRepo.GetByIdAsync(query.ConversationId, ct)
            ?? throw new InvalidOperationException("Conversation not found.");

        if (!conversation.HasActiveParticipant(query.UserId))
            throw new UnauthorizedAccessException("You are not a participant of this conversation.");

        return conversation.ToDto();
    }
}
