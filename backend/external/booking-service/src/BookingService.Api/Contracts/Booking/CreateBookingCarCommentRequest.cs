namespace BookingService.Api.Contracts.Booking;

public sealed class CreateBookingCarCommentRequest
{
    public int Rating { get; init; }
    public string Content { get; init; } = string.Empty;
}
