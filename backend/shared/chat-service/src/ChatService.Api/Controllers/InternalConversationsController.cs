using ChatService.Application.Interfaces;
using ChatService.Application.Models;
using ChatService.Domain.Entities;
using ChatService.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ChatService.Api.Hubs;
namespace ChatService.Api.Controllers;

[ApiController]
[Route("internal/conversations")]
public sealed class InternalConversationsController(
    IConversationRepository conversationRepo,
    IMessageRepository messageRepo,
    IHubContext<ConversationHub> hubContext,
    IConfiguration configuration) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateConversationRequest request, CancellationToken ct)
    {
        if (!ValidateApiKey()) return Unauthorized();

        var existing = await conversationRepo.GetByContextAsync(request.ContextType, request.ContextId, ct);
        if (existing is not null)
            return Ok(existing.ToDto());

        var conversation = new Conversation
        {
            ContextType = request.ContextType,
            ContextId = request.ContextId,
            SourceService = request.SourceService,
            Status = ConversationStatus.Open,
            Participants = request.Participants.Select(p => new ConversationParticipant
            {
                UserId = p.UserId,
                ActorType = Enum.Parse<ActorType>(p.ActorType, ignoreCase: true),
                Role = Enum.Parse<ParticipantRole>(p.Role, ignoreCase: true),
                CanRead = p.CanRead,
                CanWrite = p.CanWrite,
                CanSendInternal = p.CanSendInternal,
                Email = p.Email,
                DisplayName = p.DisplayName,
                JoinedAt = DateTime.UtcNow
            }).ToList()
        };

        await conversationRepo.CreateAsync(conversation, ct);

        if (!string.IsNullOrWhiteSpace(request.SystemMessage))
        {
            var msg = new Message
            {
                ConversationId = conversation.Id,
                SenderUserId = "system",
                SenderActorType = ActorType.System,
                MessageType = MessageType.System,
                Visibility = MessageVisibility.Participants,
                Body = request.SystemMessage,
                CreatedAt = DateTime.UtcNow
            };
            await messageRepo.CreateAsync(msg, ct);
        }

        return Created($"/conversations/{conversation.Id}", conversation.ToDto());
    }

    [HttpPost("{conversationId}/participants")]
    public async Task<IActionResult> AddParticipant(
        string conversationId, [FromBody] AddParticipantRequest request, CancellationToken ct)
    {
        if (!ValidateApiKey()) return Unauthorized();

        var conversation = await conversationRepo.GetByIdAsync(conversationId, ct);
        if (conversation is null) return NotFound();

        var existing = conversation.Participants.FirstOrDefault(p => p.UserId == request.UserId);
        if (existing is not null)
        {
            if (existing.LeftAt is not null)
            {
                existing.LeftAt = null;
                existing.CanRead = request.CanRead;
                existing.CanWrite = request.CanWrite;
                existing.CanSendInternal = request.CanSendInternal;
            }
            else
            {
                existing.CanRead = request.CanRead;
                existing.CanWrite = request.CanWrite;
                existing.CanSendInternal = request.CanSendInternal;
                existing.Role = Enum.Parse<ParticipantRole>(request.Role, ignoreCase: true);
            }
        }
        else
        {
            conversation.Participants.Add(new ConversationParticipant
            {
                UserId = request.UserId,
                ActorType = Enum.Parse<ActorType>(request.ActorType, ignoreCase: true),
                Role = Enum.Parse<ParticipantRole>(request.Role, ignoreCase: true),
                CanRead = request.CanRead,
                CanWrite = request.CanWrite,
                CanSendInternal = request.CanSendInternal,
                Email = request.Email,
                DisplayName = request.DisplayName,
                JoinedAt = DateTime.UtcNow
            });
        }

        conversation.UpdatedAt = DateTime.UtcNow;
        await conversationRepo.UpdateAsync(conversation, ct);
        return Ok(conversation.ToDto());
    }

    [HttpPatch("{conversationId}/participants/{userId}")]
    public async Task<IActionResult> UpdateParticipant(
        string conversationId, string userId, [FromBody] UpdateParticipantRequest request, CancellationToken ct)
    {
        if (!ValidateApiKey()) return Unauthorized();

        var conversation = await conversationRepo.GetByIdAsync(conversationId, ct);
        if (conversation is null) return NotFound();

        var participant = conversation.GetParticipant(userId);
        if (participant is null) return NotFound("Participant not found.");

        if (request.CanRead.HasValue) participant.CanRead = request.CanRead.Value;
        if (request.CanWrite.HasValue) participant.CanWrite = request.CanWrite.Value;
        if (request.CanSendInternal.HasValue) participant.CanSendInternal = request.CanSendInternal.Value;

        conversation.UpdatedAt = DateTime.UtcNow;
        await conversationRepo.UpdateAsync(conversation, ct);
        return Ok(conversation.ToDto());
    }

    [HttpPost("{conversationId}/close")]
    public async Task<IActionResult> Close(
        string conversationId, [FromBody] CloseConversationRequest? request, CancellationToken ct)
    {
        if (!ValidateApiKey()) return Unauthorized();

        var conversation = await conversationRepo.GetByIdAsync(conversationId, ct);
        if (conversation is null) return NotFound();

        conversation.Status = ConversationStatus.Closed;
        conversation.ClosedAt = DateTime.UtcNow;
        conversation.UpdatedAt = DateTime.UtcNow;

        foreach (var p in conversation.Participants.Where(p => p.IsActive))
            p.CanWrite = false;

        await conversationRepo.UpdateAsync(conversation, ct);

        if (!string.IsNullOrWhiteSpace(request?.SystemMessage))
        {
            var msg = new Message
            {
                ConversationId = conversationId,
                SenderUserId = "system",
                SenderActorType = ActorType.System,
                MessageType = MessageType.System,
                Visibility = MessageVisibility.Participants,
                Body = request.SystemMessage,
                CreatedAt = DateTime.UtcNow
            };
            await messageRepo.CreateAsync(msg, ct);
            await hubContext.Clients.Group(conversationId).SendAsync("NewMessage", msg.ToDto(), ct);
        }

        await hubContext.Clients.Group(conversationId)
            .SendAsync("ConversationClosed", new { conversationId }, ct);

        return Ok(conversation.ToDto());
    }

    [HttpPost("{conversationId}/reopen")]
    public async Task<IActionResult> Reopen(
        string conversationId, [FromBody] ReopenConversationRequest? request, CancellationToken ct)
    {
        if (!ValidateApiKey()) return Unauthorized();

        var conversation = await conversationRepo.GetByIdAsync(conversationId, ct);
        if (conversation is null) return NotFound();

        conversation.Status = ConversationStatus.Open;
        conversation.ClosedAt = null;
        conversation.UpdatedAt = DateTime.UtcNow;

        foreach (var p in conversation.Participants.Where(p => p.IsActive))
            p.CanWrite = true;

        await conversationRepo.UpdateAsync(conversation, ct);

        if (!string.IsNullOrWhiteSpace(request?.SystemMessage))
        {
            var msg = new Message
            {
                ConversationId = conversationId,
                SenderUserId = "system",
                SenderActorType = ActorType.System,
                MessageType = MessageType.System,
                Visibility = MessageVisibility.Participants,
                Body = request.SystemMessage,
                CreatedAt = DateTime.UtcNow
            };
            await messageRepo.CreateAsync(msg, ct);
            await hubContext.Clients.Group(conversationId).SendAsync("NewMessage", msg.ToDto(), ct);
        }

        await hubContext.Clients.Group(conversationId)
            .SendAsync("ConversationReopened", new { conversationId }, ct);

        return Ok(conversation.ToDto());
    }

    [HttpGet("by-context/{contextType}/{contextId}")]
    public async Task<IActionResult> GetByContext(string contextType, string contextId, CancellationToken ct)
    {
        if (!ValidateApiKey()) return Unauthorized();

        var conversation = await conversationRepo.GetByContextAsync(contextType, contextId, ct);
        return conversation is null ? NotFound() : Ok(conversation.ToDto());
    }

    [HttpPost("{conversationId}/system-message")]
    public async Task<IActionResult> SendSystemMessage(
        string conversationId, [FromBody] SystemMessageRequest request, CancellationToken ct)
    {
        if (!ValidateApiKey()) return Unauthorized();

        var conversation = await conversationRepo.GetByIdAsync(conversationId, ct);
        if (conversation is null) return NotFound();

        var msg = new Message
        {
            ConversationId = conversationId,
            SenderUserId = "system",
            SenderActorType = ActorType.System,
            MessageType = MessageType.System,
            Visibility = request.InternalOnly ? MessageVisibility.InternalOnly : MessageVisibility.Participants,
            Body = request.Body,
            CreatedAt = DateTime.UtcNow
        };
        await messageRepo.CreateAsync(msg, ct);

        conversation.UpdatedAt = DateTime.UtcNow;
        await conversationRepo.UpdateAsync(conversation, ct);

        await hubContext.Clients.Group(conversationId).SendAsync("NewMessage", msg.ToDto(), ct);

        return Ok(msg.ToDto());
    }

    private bool ValidateApiKey()
    {
        var expected = configuration["InternalAuth:ApiKey"];
        if (string.IsNullOrEmpty(expected)) return false;
        var provided = Request.Headers["X-Internal-Api-Key"].FirstOrDefault();
        return provided == expected;
    }
}

public sealed record CreateConversationRequest(
    string ContextType,
    string ContextId,
    string SourceService,
    List<CreateParticipantRequest> Participants,
    string? SystemMessage = null);

public sealed record CreateParticipantRequest(
    string UserId,
    string ActorType,
    string Role,
    bool CanRead = true,
    bool CanWrite = true,
    bool CanSendInternal = false,
    string? Email = null,
    string? DisplayName = null);

public sealed record AddParticipantRequest(
    string UserId,
    string ActorType,
    string Role,
    bool CanRead = true,
    bool CanWrite = true,
    bool CanSendInternal = false,
    string? Email = null,
    string? DisplayName = null);

public sealed record UpdateParticipantRequest(
    bool? CanRead = null,
    bool? CanWrite = null,
    bool? CanSendInternal = null);

public sealed record CloseConversationRequest(string? SystemMessage = null);

public sealed record SystemMessageRequest(string Body, bool InternalOnly = false);

public sealed record ReopenConversationRequest(string? SystemMessage = null);
