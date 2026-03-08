namespace BookingService.Infrastructure.Options
{
    public sealed class PendingBookingExpirationOptions
    {
        public const string SectionName = "PendingBookingExpiration";

        public int TtlMinutes { get; set; } = 15;
        public int PollIntervalSeconds { get; set; } = 30;
        public int BatchSize { get; set; } = 50;
    }
}
