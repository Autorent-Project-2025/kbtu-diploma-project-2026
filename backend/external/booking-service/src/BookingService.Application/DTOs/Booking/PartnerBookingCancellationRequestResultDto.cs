namespace BookingService.Application.DTOs.Booking;

public sealed class PartnerBookingCancellationRequestResultDto
{
    public Guid ReviewTicketId { get; set; }
    public bool AlreadyPending { get; set; }
    public BookingResponseDto Booking { get; set; } = new();
}
