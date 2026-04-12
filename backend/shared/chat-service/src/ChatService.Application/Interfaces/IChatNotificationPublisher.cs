using ChatService.Domain.Entities;

namespace ChatService.Application.Interfaces;

public interface IChatNotificationPublisher
{
    Task PublishNewMessageAsync(Conversation conversation, Message message, CancellationToken ct = default);
}
