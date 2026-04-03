namespace BookingService.Application.DTOs
{
    public sealed class BookingPricingBreakdownDto
    {
        public DateTimeOffset QuotedAtUtc { get; set; }
        public decimal MarketValueKzt { get; set; }
        public decimal Rating { get; set; }
        public int CurrentAvailableCarsCount { get; set; }
        public int DaysBeforeBooking { get; set; }
        public int BillableHours { get; set; }
        public decimal RatingCoefficient { get; set; }
        public decimal AdvanceBookingCoefficient { get; set; }
        public decimal AvailabilityCoefficient { get; set; }
        public decimal QuotedPriceHour { get; set; }
        public decimal QuotedTotalPrice { get; set; }
        public string Currency { get; set; } = "KZT";
        public bool IsMarketValueStale { get; set; }
    }
}
