namespace BookingService.Api.Contracts.Booking;

public sealed class CancelBookingRequest
{
    public string? Reason { get; init; }
}
