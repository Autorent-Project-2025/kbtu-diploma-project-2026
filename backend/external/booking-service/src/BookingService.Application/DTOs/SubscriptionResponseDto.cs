namespace BookingService.Application.DTOs.Subscription;

public class SubscriptionResponseDto
{
    public int Id { get; set; }
    public int SubscriptionPlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public bool AutoRenew { get; set; }
    public int IncludedBookings { get; set; }
    public int UsedBookings { get; set; }
    public int RemainingBookings => IncludedBookings - UsedBookings;
}