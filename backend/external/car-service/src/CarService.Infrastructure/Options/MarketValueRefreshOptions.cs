namespace CarService.Infrastructure.Options
{
    public sealed class MarketValueRefreshOptions
    {
        public const string SectionName = "MarketValueRefresh";

        public int RefreshAfterHours { get; set; } = 24;
        public int PollIntervalSeconds { get; set; } = 600;
        public int BatchSize { get; set; } = 20;
    }
}
