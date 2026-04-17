namespace IdentityService.Application.Interfaces;

public interface IEventPublisher
{
    Task PublishUserDeletedAsync(Guid userId, string email, CancellationToken cancellationToken = default);
}
