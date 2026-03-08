using PaymentService.Application.DTOs;
using PaymentService.Application.Models;

namespace PaymentService.Application.Interfaces;

public interface IPaymentLedgerService
{
    Task RecordBookingConfirmedAsync(BookingPaymentSnapshot snapshot, CancellationToken cancellationToken = default);
    Task RecordBookingCanceledAsync(int bookingId, CancellationToken cancellationToken = default);
    Task RecordBookingCompletedAsync(int bookingId, CancellationToken cancellationToken = default);
    Task<PartnerWalletResponseDto?> GetWalletAsync(Guid partnerUserId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PartnerLedgerEntryResponseDto>> GetLedgerAsync(Guid partnerUserId, int take, CancellationToken cancellationToken = default);
}
