namespace BookingService.Application.DTOs.Subscription;

public class SubscriptionPlanDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PlanType { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int IncludedBookings { get; set; }
}