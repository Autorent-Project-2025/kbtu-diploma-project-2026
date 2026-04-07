namespace BookingService.Application.Interfaces.Integrations;

public sealed class CreateCompletedBookingCarCommentPayload
{
    public int BookingId { get; set; }
    public int PartnerCarId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Content { get; set; } = string.Empty;
}
