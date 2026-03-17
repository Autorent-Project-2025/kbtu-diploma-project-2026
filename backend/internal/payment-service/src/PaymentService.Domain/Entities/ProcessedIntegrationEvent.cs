namespace PaymentService.Domain.Entities;

public sealed class ProcessedIntegrationEvent
{
    public long Id { get; set; }
    public string EventId { get; set; } = string.Empty;
    public string RoutingKey { get; set; } = string.Empty;
    public DateTimeOffset ProcessedAt { get; set; }
}
