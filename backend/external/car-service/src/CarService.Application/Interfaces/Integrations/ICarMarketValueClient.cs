namespace CarService.Application.Interfaces.Integrations
{
    public interface ICarMarketValueClient
    {
        Task<CarMarketValueEstimatePayload> GetMarketValueAsync(
            string brand,
            string model,
            int year,
            CancellationToken cancellationToken = default);
    }

    public sealed class CarMarketValueEstimatePayload
    {
        public decimal MarketValueKzt { get; init; }
        public int SampleCount { get; init; }
        public int FilteredSampleCount { get; init; }
        public string Confidence { get; init; } = string.Empty;
        public string Source { get; init; } = string.Empty;
        public string SourceUrl { get; init; } = string.Empty;
        public DateTimeOffset FetchedAt { get; init; }
    }
}
