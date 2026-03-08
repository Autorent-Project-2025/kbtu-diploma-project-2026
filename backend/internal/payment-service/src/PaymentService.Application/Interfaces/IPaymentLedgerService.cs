using PaymentService.Application.DTOs;
using PaymentService.Application.Models;

namespace PaymentService.Application.Interfaces;

public interface IPaymentLedgerService
{
    Task RecordBookingConfirmedAsync(BookingPaymentSnapshot snapshot, CancellationToken cancellationToken = default);
    Task RecordBookingCanceledAsync(int bookingId, CancellationToken cancellationToken = default);
    Task RecordBookingCompletedAsync(int bookingId, CancellationToken cancellationToken = default);
    Task<PartnerPayoutResponseDto> RequestPayoutAsync(Guid partnerUserId, decimal amount, string requestKey, CancellationToken cancellationToken = default);
    Task<PartnerPayoutResponseDto> MarkPayoutProcessingAsync(long payoutId, CancellationToken cancellationToken = default);
    Task<PartnerPayoutResponseDto> MarkPayoutPaidAsync(long payoutId, CancellationToken cancellationToken = default);
    Task<PartnerPayoutResponseDto> MarkPayoutFailedAsync(long payoutId, string? failureReason, CancellationToken cancellationToken = default);
    Task<PartnerPayoutResponseDto> CancelPayoutAsync(long payoutId, string? reason, CancellationToken cancellationToken = default);
    Task<PartnerPayoutResponseDto?> GetPayoutAsync(long payoutId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PartnerPayoutResponseDto>> GetPayoutsAsync(Guid partnerUserId, int take, CancellationToken cancellationToken = default);
    Task<PartnerWalletResponseDto?> GetWalletAsync(Guid partnerUserId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PartnerLedgerEntryResponseDto>> GetLedgerAsync(Guid partnerUserId, int take, CancellationToken cancellationToken = default);
}
