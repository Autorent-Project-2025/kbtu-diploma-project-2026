namespace BookingService.Infrastructure.Integrations
{
    internal static class PaymentSyncOutboxEventTypes
    {
        public const string BookingConfirmed = "booking.confirmed";
        public const string BookingCanceled = "booking.canceled";
        public const string BookingCompleted = "booking.completed";

        // Emitted when a booking is created (status=Pending) so payment-service
        // can asynchronously provision a mock payment session. Replaces the
        // former synchronous HTTP call to /internal/mock-payments/start.
        public const string BookingPaymentSessionRequested = "booking.payment-session-requested";
    }
}
