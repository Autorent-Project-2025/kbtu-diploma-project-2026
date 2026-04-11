using ChatService.Application.Interfaces;
using ChatService.Application.Models;
using ChatService.Domain.Entities;
using ChatService.Domain.Enums;

namespace ChatService.Application.Conversations.Commands.SendMessage;

public sealed class SendMessageCommandHandler(
    IConversationRepository conversationRepo,
    IMessageRepository messageRepo,
    IFileServiceClient fileServiceClient)
{
    public async Task<MessageDto> HandleAsync(SendMessageCommand command, CancellationToken ct = default)
    {
        var conversation = await conversationRepo.GetByIdAsync(command.ConversationId, ct)
            ?? throw new InvalidOperationException("Conversation not found.");

        if (!conversation.IsOpen)
            throw new InvalidOperationException("Conversation is closed.");

        var participant = conversation.GetParticipant(command.SenderUserId)
            ?? throw new UnauthorizedAccessException("You are not a participant of this conversation.");

        if (!participant.CanWrite)
            throw new UnauthorizedAccessException("You do not have write access.");

        if (command.Visibility == MessageVisibility.InternalOnly && !participant.CanSendInternal)
            throw new UnauthorizedAccessException("You cannot send internal messages.");

        var hasAttachments = command.Attachments is { Count: > 0 };

        if (string.IsNullOrWhiteSpace(command.Body) && !hasAttachments && command.MessageType != MessageType.System)
            throw new InvalidOperationException("Message body cannot be empty.");

        if (command.Body.Length > 4000)
            throw new InvalidOperationException("Message body cannot exceed 4000 characters.");

        var message = new Message
        {
            ConversationId = command.ConversationId,
            SenderUserId = command.SenderUserId,
            SenderActorType = command.SenderActorType,
            MessageType = command.MessageType,
            Visibility = command.Visibility,
            Body = command.Body,
            CreatedAt = DateTime.UtcNow
        };

        if (hasAttachments)
        {
            foreach (var file in command.Attachments!.Take(10))
            {
                var result = await fileServiceClient.UploadFileAsync(
                    file.Stream, file.FileName, file.ContentType, ct);

                message.Attachments.Add(new MessageAttachment
                {
                    FileName = result.FileName,
                    OriginalFileName = result.OriginalFileName,
                    MimeType = file.ContentType,
                    UploadedByUserId = command.SenderUserId,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await messageRepo.CreateAsync(message, ct);

        conversation.UpdatedAt = DateTime.UtcNow;
        await conversationRepo.UpdateAsync(conversation, ct);

        return message.ToDto();
    }
}
