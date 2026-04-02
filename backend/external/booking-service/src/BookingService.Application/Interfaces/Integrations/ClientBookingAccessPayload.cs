namespace BookingService.Application.Interfaces.Integrations;

public sealed class ClientBookingAccessPayload
{
    public string RelatedUserId { get; set; } = string.Empty;
    public bool BookingActionsBlocked { get; set; }
    public string? BookingBlockReason { get; set; }
    public DateTimeOffset? BookingBlockedAt { get; set; }
}
