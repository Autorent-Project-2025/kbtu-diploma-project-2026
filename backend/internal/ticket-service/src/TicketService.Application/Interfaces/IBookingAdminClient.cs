namespace TicketService.Application.Interfaces;

public interface IBookingAdminClient
{
    Task<bool> CancelBookingAsync(int bookingId, CancellationToken cancellationToken = default);
}
