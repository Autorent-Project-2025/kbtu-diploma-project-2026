namespace ChatService.Application.Models;

public sealed record MessageDto(
    string Id,
    string ConversationId,
    string SenderUserId,
    string SenderActorType,
    string MessageType,
    string Visibility,
    string Body,
    DateTime CreatedAt,
    List<AttachmentDto> Attachments);

public sealed record AttachmentDto(
    string Id,
    string FileName,
    string OriginalFileName,
    string MimeType,
    DateTime CreatedAt);
