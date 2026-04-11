using ChatService.Domain.Enums;

namespace ChatService.Application.Conversations.Commands.SendMessage;

public sealed record SendMessageCommand(
    string ConversationId,
    string SenderUserId,
    ActorType SenderActorType,
    MessageType MessageType,
    MessageVisibility Visibility,
    string Body,
    List<AttachmentUpload>? Attachments = null);

public sealed record AttachmentUpload(
    Stream Stream,
    string FileName,
    string ContentType);
