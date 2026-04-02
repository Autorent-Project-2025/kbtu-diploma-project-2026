namespace BookingService.Application.DTOs.Booking;

public sealed class BookingCompletionSubmissionResponseDto
{
    public BookingResponseDto Booking { get; set; } = new();
    public Guid ReviewTicketId { get; set; }
    public decimal LatePenaltyAmount { get; set; }
}
