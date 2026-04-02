namespace BookingService.Application.Interfaces.Integrations;

public interface IIdentityUserReadClient
{
    Task<IdentityUserPayload?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
