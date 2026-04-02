namespace BookingService.Application.Interfaces.Integrations
{
    public interface IPaymentSyncClient
    {
        Task<MockPaymentAttemptPayload> StartMockPaymentAsync(
            int bookingId,
            Guid userId,
            decimal amount,
            string currency,
            CancellationToken cancellationToken = default);

        Task<MockPaymentAttemptPayload?> GetLatestMockPaymentAsync(
            int bookingId,
            Guid userId,
            CancellationToken cancellationToken = default);

        Task<MockPaymentAttemptPayload> SubmitMockPaymentAsync(
            int bookingId,
            Guid userId,
            string sessionKey,
            string cardHolder,
            string cardNumber,
            int expiryMonth,
            int expiryYear,
            string cvv,
            CancellationToken cancellationToken = default);

        Task RecordBookingConfirmedAsync(
            int bookingId,
            Guid userId,
            Guid partnerUserId,
            int partnerCarId,
            decimal? priceHour,
            decimal? totalPrice,
            CancellationToken cancellationToken = default);

        Task RecordBookingCanceledAsync(int bookingId, CancellationToken cancellationToken = default);

        Task RecordBookingCompletedAsync(int bookingId, CancellationToken cancellationToken = default);

        Task<BookingChargePayload> CreateBookingChargeAsync(
            int bookingId,
            Guid userId,
            Guid partnerUserId,
            string chargeType,
            decimal amount,
            string? description = null,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<BookingChargePayload>> GetBookingChargesAsync(
            int bookingId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<BookingChargePayload>> GetUserBookingChargesAsync(
            Guid userId,
            string? chargeType = null,
            string? status = null,
            CancellationToken cancellationToken = default);

        Task<BookingChargePayload> MarkBookingChargePaidAsync(
            long chargeId,
            CancellationToken cancellationToken = default);
    }
}
