namespace BookingService.Api.Contracts.Booking;

public sealed class PartnerCancelBookingRequest
{
    public string? Reason { get; init; }
}
