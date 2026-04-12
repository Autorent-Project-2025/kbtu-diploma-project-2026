namespace TicketService.Application.Interfaces;

public interface IBookingAdminClient
{
    Task<bool> CancelBookingAsync(int bookingId, string? cancellationReason = null, CancellationToken cancellationToken = default);
    Task<bool> ApprovePartnerCancellationAsync(
        int bookingId,
        Guid ticketId,
        string partnerReason,
        CancellationToken cancellationToken = default);
    Task<bool> RejectPartnerCancellationAsync(
        int bookingId,
        Guid ticketId,
        string decisionReason,
        CancellationToken cancellationToken = default);
}
