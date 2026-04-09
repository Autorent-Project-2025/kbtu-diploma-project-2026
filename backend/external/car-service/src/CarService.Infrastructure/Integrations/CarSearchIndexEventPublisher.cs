using AutoRent.Messaging.Contracts;
using AutoRent.Messaging.RabbitMq;
using CarService.Application.Interfaces.Integrations;
using Microsoft.Extensions.Logging;

namespace CarService.Infrastructure.Integrations;

public sealed class CarSearchIndexEventPublisher : ICarSearchIndexEventPublisher
{
    private readonly IRabbitMqPublisher _rabbitMqPublisher;
    private readonly ILogger<CarSearchIndexEventPublisher> _logger;

    public CarSearchIndexEventPublisher(
        IRabbitMqPublisher rabbitMqPublisher,
        ILogger<CarSearchIndexEventPublisher> logger)
    {
        _rabbitMqPublisher = rabbitMqPublisher;
        _logger = logger;
    }

    public async Task PublishUpsertRequestedAsync(int partnerCarId, CancellationToken cancellationToken = default)
    {
        await PublishAsync(
            partnerCarId,
            RabbitMqTopology.RoutingKeys.CarSearchPartnerCarUpsertRequested,
            "upsert",
            cancellationToken);
    }

    public async Task PublishDeletedAsync(int partnerCarId, CancellationToken cancellationToken = default)
    {
        await PublishAsync(
            partnerCarId,
            RabbitMqTopology.RoutingKeys.CarSearchPartnerCarDeleted,
            "deleted",
            cancellationToken);
    }

    private async Task PublishAsync(
        int partnerCarId,
        string routingKey,
        string changeType,
        CancellationToken cancellationToken)
    {
        if (partnerCarId <= 0)
        {
            return;
        }

        try
        {
            await _rabbitMqPublisher.PublishAsync(
                eventId: $"car-search:{partnerCarId}:{changeType}",
                routingKey: routingKey,
                payload: new PartnerCarSearchDocumentChanged(
                    PartnerCarId: partnerCarId,
                    ChangeType: changeType),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to publish car search indexing event {RoutingKey} for partner car {PartnerCarId}.",
                routingKey,
                partnerCarId);
        }
    }
}
