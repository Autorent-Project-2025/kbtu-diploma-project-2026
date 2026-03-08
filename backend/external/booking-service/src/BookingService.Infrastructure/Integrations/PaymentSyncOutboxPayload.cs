namespace BookingService.Infrastructure.Integrations
{
    public sealed class PaymentSyncOutboxPayload
    {
        public int BookingId { get; init; }
        public Guid? UserId { get; init; }
        public Guid? PartnerUserId { get; init; }
        public int? PartnerCarId { get; init; }
        public decimal? PriceHour { get; init; }
        public decimal? TotalPrice { get; init; }
    }
}
