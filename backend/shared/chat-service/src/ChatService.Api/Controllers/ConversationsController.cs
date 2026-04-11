using System.Security.Claims;
using ChatService.Application.Conversations.Commands.SendMessage;
using ChatService.Application.Conversations.Queries.GetConversation;
using ChatService.Application.Conversations.Queries.GetConversationByContext;
using ChatService.Application.Conversations.Queries.GetMessages;
using ChatService.Application.Interfaces;
using ChatService.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ChatService.Api.Hubs;

namespace ChatService.Api.Controllers;

[ApiController]
[Route("conversations")]
[Authorize]
public sealed class ConversationsController(
    GetConversationQueryHandler getConversationHandler,
    GetConversationByContextQueryHandler getByContextHandler,
    GetMessagesQueryHandler getMessagesHandler,
    SendMessageCommandHandler sendMessageHandler,
    IConversationRepository conversationRepo,
    IMessageRepository messageRepo,
    IFileServiceClient fileServiceClient,
    IChatNotificationPublisher notificationPublisher,
    IHubContext<ConversationHub> hubContext) : ControllerBase
{
    [HttpGet("by-context/{contextType}/{contextId}")]
    public async Task<IActionResult> GetByContext(string contextType, string contextId, CancellationToken ct)
    {
        var userId = ResolveUserId();
        var result = await getByContextHandler.HandleAsync(
            new GetConversationByContextQuery(contextType, contextId, userId), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{conversationId}")]
    public async Task<IActionResult> Get(string conversationId, CancellationToken ct)
    {
        var userId = ResolveUserId();
        var result = await getConversationHandler.HandleAsync(
            new GetConversationQuery(conversationId, userId), ct);
        return Ok(result);
    }

    [HttpGet("{conversationId}/messages")]
    public async Task<IActionResult> GetMessages(
        string conversationId, [FromQuery] string? before, [FromQuery] int limit = 50, CancellationToken ct = default)
    {
        var userId = ResolveUserId();
        var result = await getMessagesHandler.HandleAsync(
            new GetMessagesQuery(conversationId, userId, before, limit), ct);
        return Ok(result);
    }

    [HttpPost("{conversationId}/messages")]
    public async Task<IActionResult> SendMessage(
        string conversationId, [FromForm] SendMessageFormRequest request, CancellationToken ct)
    {
        var userId = ResolveUserId();
        var actorType = ResolveActorType();

        var isInternal = request.Internal;
        var visibility = isInternal ? MessageVisibility.InternalOnly : MessageVisibility.Participants;
        var messageType = isInternal ? MessageType.InternalNote : MessageType.Text;

        List<AttachmentUpload>? attachments = null;
        if (request.Files is { Count: > 0 })
        {
            attachments = request.Files.Select(f => new AttachmentUpload(
                f.OpenReadStream(), f.FileName, f.ContentType)).ToList();
        }

        var result = await sendMessageHandler.HandleAsync(new SendMessageCommand(
            conversationId, userId, actorType, messageType, visibility,
            request.Body ?? string.Empty, attachments), ct);

        if (attachments is not null)
        {
            foreach (var a in attachments)
                a.Stream.Dispose();
        }

        await hubContext.Clients.Group(conversationId).SendAsync("NewMessage", result, ct);

        // Fire-and-forget email notification for offline participants
        _ = Task.Run(async () =>
        {
            try
            {
                var conv = await conversationRepo.GetByIdAsync(conversationId, CancellationToken.None);
                if (conv is not null)
                {
                    var msg = await messageRepo.GetByIdAsync(result.Id, CancellationToken.None);
                    if (msg is not null)
                        await notificationPublisher.PublishNewMessageAsync(conv, msg, CancellationToken.None);
                }
            }
            catch { /* non-blocking */ }
        });

        return Ok(result);
    }

    [HttpGet("{conversationId}/attachments/{attachmentId}/temporary-link")]
    public async Task<IActionResult> GetAttachmentTemporaryLink(
        string conversationId, string attachmentId, CancellationToken ct)
    {
        var userId = ResolveUserId();

        var conversation = await conversationRepo.GetByIdAsync(conversationId, ct)
            ?? throw new KeyNotFoundException("Conversation not found.");

        var participant = conversation.GetParticipant(userId)
            ?? throw new UnauthorizedAccessException("You are not a participant of this conversation.");

        if (!participant.CanRead)
            throw new UnauthorizedAccessException("You do not have read access.");

        var message = await messageRepo.GetByAttachmentIdAsync(conversationId, attachmentId, ct)
            ?? throw new KeyNotFoundException("Attachment not found.");

        if (!message.IsVisibleTo(participant))
            throw new UnauthorizedAccessException("You do not have access to this attachment.");

        var attachment = message.Attachments.First(a => a.Id == attachmentId);
        var url = await fileServiceClient.GetTemporaryLinkAsync(attachment.FileName, 900, ct);

        return Ok(new { url, fileName = attachment.OriginalFileName, mimeType = attachment.MimeType });
    }

    private string ResolveUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException("User ID not found in token.");
    }

    private ActorType ResolveActorType()
    {
        var actorTypeStr = User.FindFirstValue("actor_type") ?? "manager";
        return actorTypeStr.ToLowerInvariant() switch
        {
            "client" => ActorType.Client,
            "partner" => ActorType.Partner,
            "manager" => ActorType.Manager,
            "supermanager" => ActorType.Supermanager,
            "admin" => ActorType.Admin,
            _ => ActorType.Manager
        };
    }
}

public sealed class SendMessageFormRequest
{
    public string? Body { get; set; }
    public bool Internal { get; set; } = false;
    public List<IFormFile>? Files { get; set; }
}
