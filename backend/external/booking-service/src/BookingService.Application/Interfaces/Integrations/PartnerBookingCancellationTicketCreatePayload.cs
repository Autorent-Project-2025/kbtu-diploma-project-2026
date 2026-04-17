namespace BookingService.Application.Interfaces.Integrations;

public sealed class PartnerBookingCancellationTicketCreatePayload
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public Guid RelatedPartnerUserId { get; set; }
    public int BookingId { get; set; }
    public string CarBrand { get; set; } = string.Empty;
    public string CarModel { get; set; } = string.Empty;
    public string BookingStatus { get; set; } = string.Empty;
    public DateTimeOffset BookingStartTime { get; set; }
    public DateTimeOffset BookingEndTime { get; set; }
    public string PartnerReason { get; set; } = string.Empty;
}
