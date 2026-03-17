namespace AutoRent.Messaging.RabbitMq;

public sealed record IntegrationMessage<TPayload>(
    string EventId,
    string RoutingKey,
    DateTimeOffset OccurredAtUtc,
    TPayload Payload);
