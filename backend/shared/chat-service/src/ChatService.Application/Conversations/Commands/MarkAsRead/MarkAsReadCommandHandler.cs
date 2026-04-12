using ChatService.Application.Interfaces;

namespace ChatService.Application.Conversations.Commands.MarkAsRead;

public sealed class MarkAsReadCommandHandler(
    IConversationRepository conversationRepo,
    IMessageRepository messageRepo)
{
    public async Task HandleAsync(MarkAsReadCommand command, CancellationToken ct = default)
    {
        var conversation = await conversationRepo.GetByIdAsync(command.ConversationId, ct)
            ?? throw new InvalidOperationException("Conversation not found.");

        var participant = conversation.GetParticipant(command.UserId)
            ?? throw new UnauthorizedAccessException("You are not a participant of this conversation.");

        if (!participant.CanRead)
            throw new UnauthorizedAccessException("You do not have read access.");

        var message = await messageRepo.GetByIdAsync(command.MessageId, ct)
            ?? throw new InvalidOperationException("Message not found.");

        if (message.ConversationId != command.ConversationId)
            throw new InvalidOperationException("Message does not belong to this conversation.");

        participant.LastReadMessageId = command.MessageId;
        participant.LastReadAt = DateTime.UtcNow;

        await conversationRepo.UpdateAsync(conversation, ct);
    }
}
