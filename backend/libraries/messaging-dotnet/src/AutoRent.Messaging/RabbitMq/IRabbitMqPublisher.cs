namespace AutoRent.Messaging.RabbitMq;

public interface IRabbitMqPublisher
{
    Task PublishAsync<TPayload>(
        string eventId,
        string routingKey,
        TPayload payload,
        CancellationToken cancellationToken = default);
}
