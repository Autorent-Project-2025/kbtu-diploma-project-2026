using AutoRent.Messaging.Contracts;
using AutoRent.Messaging.RabbitMq;
using IdentityService.Application.Interfaces;

namespace IdentityService.Infrastructure.Events;

public sealed class RabbitMqEventPublisher : IEventPublisher
{
    private readonly IRabbitMqPublisher _publisher;

    public RabbitMqEventPublisher(IRabbitMqPublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task PublishUserDeletedAsync(Guid userId, string email, CancellationToken cancellationToken = default)
    {
        await _publisher.PublishAsync(
            userId.ToString(),
            RabbitMqTopology.RoutingKeys.UserDeleted,
            new UserDeleted(userId, email),
            cancellationToken);
    }
}
