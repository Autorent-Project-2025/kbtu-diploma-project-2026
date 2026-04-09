namespace CarService.Application.Interfaces.Integrations;

public interface ICarSearchIndexEventPublisher
{
    Task PublishUpsertRequestedAsync(int partnerCarId, CancellationToken cancellationToken = default);
    Task PublishDeletedAsync(int partnerCarId, CancellationToken cancellationToken = default);
}
