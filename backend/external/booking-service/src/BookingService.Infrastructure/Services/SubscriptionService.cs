using BookingService.Application.DTOs.Subscription;
using BookingService.Application.Interfaces;
using BookingService.Domain.Entities;
using BookingService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Infrastructure.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly ApplicationDbContext _db;

    public SubscriptionService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyCollection<SubscriptionPlanDto>> GetPlans(CancellationToken cancellationToken = default)
    {
        return await _db.SubscriptionPlans
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Price)
            .Select(x => new SubscriptionPlanDto
            {
                Id = x.Id,
                Name = x.Name,
                PlanType = x.PlanType,
                Price = x.Price,
                IncludedBookings = x.IncludedBookings
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<SubscriptionResponseDto> CreateSubscription(
        Guid userId,
        CreateSubscriptionDto dto,
        CancellationToken cancellationToken = default)
    {
        var existing = await _db.Subscriptions
            .FirstOrDefaultAsync(x =>
                x.UserId == userId &&
                x.Status == "active" &&
                x.EndDate > DateTimeOffset.UtcNow,
                cancellationToken);

        if (existing != null)
            throw new InvalidOperationException("User already has an active subscription.");

        var plan = await _db.SubscriptionPlans
            .FirstOrDefaultAsync(x => x.Id == dto.SubscriptionPlanId && x.IsActive, cancellationToken);

        if (plan == null)
            throw new KeyNotFoundException("Subscription plan not found.");

        var now = DateTimeOffset.UtcNow;
        var endDate = plan.PlanType.ToLowerInvariant() switch
        {
            "weekly" => now.AddDays(7),
            "monthly" => now.AddMonths(1),
            _ => now.AddMonths(1)
        };

        var subscription = new Subscription
        {
            UserId = userId,
            SubscriptionPlanId = plan.Id,
            Status = "active",
            StartDate = now,
            EndDate = endDate,
            AutoRenew = dto.AutoRenew,
            IncludedBookings = plan.IncludedBookings,
            UsedBookings = 0,
            CreatedAt = now
        };

        _db.Subscriptions.Add(subscription);
        await _db.SaveChangesAsync(cancellationToken);

        return new SubscriptionResponseDto
        {
            Id = subscription.Id,
            SubscriptionPlanId = plan.Id,
            PlanName = plan.Name,
            Status = subscription.Status,
            StartDate = subscription.StartDate,
            EndDate = subscription.EndDate,
            AutoRenew = subscription.AutoRenew,
            IncludedBookings = subscription.IncludedBookings,
            UsedBookings = subscription.UsedBookings
        };
    }

    public async Task<SubscriptionResponseDto?> GetActiveSubscription(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var subscription = await _db.Subscriptions
            .AsNoTracking()
            .Include(x => x.Plan)
            .FirstOrDefaultAsync(x =>
                x.UserId == userId &&
                x.Status == "active" &&
                x.EndDate > DateTimeOffset.UtcNow,
                cancellationToken);

        if (subscription == null)
            return null;

        return new SubscriptionResponseDto
        {
            Id = subscription.Id,
            SubscriptionPlanId = subscription.SubscriptionPlanId,
            PlanName = subscription.Plan.Name,
            Status = subscription.Status,
            StartDate = subscription.StartDate,
            EndDate = subscription.EndDate,
            AutoRenew = subscription.AutoRenew,
            IncludedBookings = subscription.IncludedBookings,
            UsedBookings = subscription.UsedBookings
        };
    }

    public async Task<bool> CancelSubscription(
        int id,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var subscription = await _db.Subscriptions
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);

        if (subscription == null)
            return false;

        subscription.Status = "cancelled";
        subscription.AutoRenew = false;

        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> TryUseSubscription(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var subscription = await _db.Subscriptions
            .FirstOrDefaultAsync(x =>
                x.UserId == userId &&
                x.Status == "active" &&
                x.EndDate > DateTimeOffset.UtcNow,
                cancellationToken);

        if (subscription == null)
            return false;

        if (subscription.UsedBookings >= subscription.IncludedBookings)
            return false;

        subscription.UsedBookings += 1;
        await _db.SaveChangesAsync(cancellationToken);

        return true;
    }
}