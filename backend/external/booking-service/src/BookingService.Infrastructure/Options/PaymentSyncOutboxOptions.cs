namespace BookingService.Infrastructure.Options
{
    public sealed class PaymentSyncOutboxOptions
    {
        public const string SectionName = "PaymentSyncOutbox";

        public int BatchSize { get; set; } = 20;
        public int PollIntervalSeconds { get; set; } = 5;
        public int LockTimeoutSeconds { get; set; } = 30;
        public int InitialRetryDelaySeconds { get; set; } = 5;
        public int MaxRetryDelaySeconds { get; set; } = 300;
    }
}
