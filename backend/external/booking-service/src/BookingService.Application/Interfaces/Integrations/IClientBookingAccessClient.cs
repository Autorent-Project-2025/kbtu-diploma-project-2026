namespace BookingService.Application.Interfaces.Integrations;

public interface IClientBookingAccessClient
{
    Task<ClientBookingAccessPayload?> GetBookingAccessAsync(Guid relatedUserId, CancellationToken cancellationToken = default);
    Task<ClientProfilePayload?> GetClientProfileAsync(Guid relatedUserId, CancellationToken cancellationToken = default);
    Task<ClientProfilePayload?> SetBookingActionsBlockedAsync(
        Guid relatedUserId,
        bool isBlocked,
        string? reason,
        CancellationToken cancellationToken = default);
}
