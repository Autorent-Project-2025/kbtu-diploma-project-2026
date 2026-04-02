namespace BookingService.Infrastructure.Options
{
    public sealed class UnstartedBookingExpirationOptions
    {
        public const string SectionName = "UnstartedBookingExpiration";

        public int PollIntervalSeconds { get; set; } = 30;
        public int BatchSize { get; set; } = 50;
    }
}
