namespace CarService.Application.DTOs.Matching
{
    public sealed class PriceEstimateResponseDto
    {
        public decimal MarketValueKzt { get; init; }
        public decimal PriceHour { get; init; }
        public decimal PriceDay { get; init; }
        public string Confidence { get; init; } = string.Empty;
        public int SampleCount { get; init; }
    }
}
