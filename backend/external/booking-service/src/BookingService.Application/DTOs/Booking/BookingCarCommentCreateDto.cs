namespace BookingService.Application.DTOs.Booking;

public sealed class BookingCarCommentCreateDto
{
    public int Rating { get; set; }
    public string Content { get; set; } = string.Empty;
}
