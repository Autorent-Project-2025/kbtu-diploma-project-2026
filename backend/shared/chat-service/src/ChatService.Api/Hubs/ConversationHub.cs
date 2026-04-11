using System.Security.Claims;
using ChatService.Application.Conversations.Commands.MarkAsRead;
using ChatService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatService.Api.Hubs;

[Authorize]
public sealed class ConversationHub(
    IConversationRepository conversationRepo,
    MarkAsReadCommandHandler markAsReadHandler) : Hub
{
    public async Task JoinConversation(string conversationId)
    {
        var userId = GetUserId();
        var conversation = await conversationRepo.GetByIdAsync(conversationId);

        if (conversation is null || !conversation.HasActiveParticipant(userId))
        {
            await Clients.Caller.SendAsync("Error", "Access denied.");
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
        await Clients.Caller.SendAsync("JoinedConversation", conversationId);

        // Notify others that user is online
        await Clients.OthersInGroup(conversationId)
            .SendAsync("UserPresence", new { userId, isOnline = true });
    }

    public async Task LeaveConversation(string conversationId)
    {
        var userId = GetUserId();
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId);

        await Clients.OthersInGroup(conversationId)
            .SendAsync("UserPresence", new { userId, isOnline = false });
    }

    public async Task StartTyping(string conversationId)
    {
        var userId = GetUserId();
        await Clients.OthersInGroup(conversationId)
            .SendAsync("UserTyping", new { userId, isTyping = true });
    }

    public async Task StopTyping(string conversationId)
    {
        var userId = GetUserId();
        await Clients.OthersInGroup(conversationId)
            .SendAsync("UserTyping", new { userId, isTyping = false });
    }

    public async Task MarkAsRead(string conversationId, string messageId)
    {
        var userId = GetUserId();
        try
        {
            await markAsReadHandler.HandleAsync(
                new MarkAsReadCommand(conversationId, userId, messageId));

            await Clients.OthersInGroup(conversationId)
                .SendAsync("ReadStateUpdated", new { userId, conversationId, messageId });
        }
        catch
        {
            // Silently ignore read-state errors
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Note: SignalR automatically removes the connection from groups on disconnect.
        // Presence is ephemeral; group membership cleanup is handled by SignalR.
        await base.OnDisconnectedAsync(exception);
    }

    private string GetUserId()
    {
        return Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? Context.User?.FindFirstValue("sub")
            ?? throw new HubException("Unauthorized");
    }
}
