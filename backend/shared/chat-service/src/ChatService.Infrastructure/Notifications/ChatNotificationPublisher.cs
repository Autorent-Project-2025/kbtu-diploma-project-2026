using AutoRent.Messaging.RabbitMq;
using ChatService.Application.Interfaces;
using ChatService.Domain.Entities;
using ChatService.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace ChatService.Infrastructure.Notifications;

public sealed class ChatNotificationPublisher : IChatNotificationPublisher
{
    private const string RoutingKey = "chat.email.new-message";

    private readonly IRabbitMqPublisher _publisher;
    private readonly ILogger<ChatNotificationPublisher> _logger;

    public ChatNotificationPublisher(
        IRabbitMqPublisher publisher,
        ILogger<ChatNotificationPublisher> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    public async Task PublishNewMessageAsync(Conversation conversation, Message message, CancellationToken ct = default)
    {
        if (message.MessageType == MessageType.System)
            return;

        var recipients = conversation.Participants
            .Where(p => p.IsActive
                && p.UserId != message.SenderUserId
                && p.CanRead
                && !string.IsNullOrEmpty(p.Email)
                && message.IsVisibleTo(p))
            .ToList();

        if (recipients.Count == 0)
            return;

        foreach (var recipient in recipients)
        {
            try
            {
                var payload = new ChatNewMessageEmailRequested(
                    ConversationId: conversation.Id,
                    ContextType: conversation.ContextType,
                    ContextId: conversation.ContextId,
                    To: recipient.Email!,
                    RecipientName: recipient.DisplayName ?? recipient.UserId,
                    SenderName: GetSenderName(conversation, message.SenderUserId),
                    MessagePreview: TruncateBody(message.Body, 200));

                await _publisher.PublishAsync(
                    Guid.NewGuid().ToString(),
                    RoutingKey,
                    payload,
                    ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to publish notification for conversation {ConversationId} to {UserId}",
                    conversation.Id, recipient.UserId);
            }
        }
    }

    private static string GetSenderName(Conversation conversation, string senderUserId)
    {
        var participant = conversation.Participants.FirstOrDefault(p => p.UserId == senderUserId);
        return participant?.DisplayName ?? participant?.ActorType.ToString() ?? "Unknown";
    }

    private static string TruncateBody(string body, int maxLength) =>
        body.Length <= maxLength ? body : body[..maxLength] + "...";
}

public sealed record ChatNewMessageEmailRequested(
    string ConversationId,
    string ContextType,
    string ContextId,
    string To,
    string RecipientName,
    string SenderName,
    string MessagePreview);
