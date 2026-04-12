using TicketService.Application.Models;

namespace TicketService.Application.Interfaces;

public interface IBookingReadClient
{
    Task<BookingForComplaintResult?> GetBookingAsync(
        int bookingId,
        CancellationToken cancellationToken = default);
}
