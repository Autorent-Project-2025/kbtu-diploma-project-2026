namespace TicketService.Application.Interfaces;

public interface IBookingAdminClient
{
    Task<bool> CancelBookingAsync(int bookingId, CancellationToken cancellationToken = default);
    Task<bool> ApprovePartnerCancellationAsync(
        int bookingId,
        Guid ticketId,
        CancellationToken cancellationToken = default);
    Task<bool> RejectPartnerCancellationAsync(
        int bookingId,
        Guid ticketId,
        string decisionReason,
        CancellationToken cancellationToken = default);
}
