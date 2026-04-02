namespace BookingService.Application.Interfaces.Integrations;

public sealed class ClientProfilePayload
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string RelatedUserId { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool BookingActionsBlocked { get; set; }
    public string? BookingBlockReason { get; set; }
    public DateTimeOffset? BookingBlockedAt { get; set; }
}
