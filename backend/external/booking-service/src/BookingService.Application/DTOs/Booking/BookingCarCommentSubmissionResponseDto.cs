namespace BookingService.Application.DTOs.Booking;

public sealed class BookingCarCommentSubmissionResponseDto
{
    public BookingResponseDto Booking { get; set; } = new();
    public int CommentId { get; set; }
    public DateTimeOffset SubmittedAt { get; set; }
}
