using System.ComponentModel.DataAnnotations;

namespace BookingService.Application.DTOs.Subscription;

public class CreateSubscriptionDto
{
    [Range(1, int.MaxValue)]
    public int SubscriptionPlanId { get; set; }

    public bool AutoRenew { get; set; }
}