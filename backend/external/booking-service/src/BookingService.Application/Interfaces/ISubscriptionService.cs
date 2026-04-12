using BookingService.Application.DTOs.Subscription;

namespace BookingService.Application.Interfaces;

public interface ISubscriptionService
{
    Task<IReadOnlyCollection<SubscriptionPlanDto>> GetPlans(CancellationToken cancellationToken = default);

    Task<SubscriptionResponseDto> CreateSubscription(
        Guid userId,
        CreateSubscriptionDto dto,
        CancellationToken cancellationToken = default);

    Task<SubscriptionResponseDto?> GetActiveSubscription(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<bool> CancelSubscription(
        int id,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<bool> TryUseSubscription(
        Guid userId,
        CancellationToken cancellationToken = default);
}