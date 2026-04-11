namespace TicketService.Application.Interfaces;

public interface IPaymentClient
{
    Task<bool> CancelBookingChargeAsync(long chargeId, string? reason = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<BookingChargeInfo>> GetBookingChargesAsync(int bookingId, CancellationToken cancellationToken = default);
}

public sealed record BookingChargeInfo(
    long Id,
    int BookingId,
    string ChargeType,
    decimal Amount,
    string Status,
    string? Description);
