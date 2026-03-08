namespace BookingService.Infrastructure.Integrations
{
    internal static class PaymentSyncOutboxEventTypes
    {
        public const string BookingConfirmed = "booking.confirmed";
        public const string BookingCanceled = "booking.canceled";
        public const string BookingCompleted = "booking.completed";
    }
}
