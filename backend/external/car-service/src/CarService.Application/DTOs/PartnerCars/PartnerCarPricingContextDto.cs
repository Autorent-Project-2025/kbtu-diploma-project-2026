namespace CarService.Application.DTOs.PartnerCars
{
    public sealed class PartnerCarPricingContextDto
    {
        public int PartnerCarId { get; set; }
        public Guid PartnerUserId { get; set; }
        public int CarModelId { get; set; }
        public decimal? MarketValueKzt { get; set; }
        public DateTimeOffset? MarketValueFetchedAt { get; set; }
        public bool IsMarketValueStale { get; set; }
        public decimal Rating { get; set; }
        public int CurrentAvailableCarsCount { get; set; }
    }
}
