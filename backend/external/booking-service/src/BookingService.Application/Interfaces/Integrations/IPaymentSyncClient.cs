namespace BookingService.Application.Interfaces.Integrations
{
    public interface IPaymentSyncClient
    {
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
    }
}
