namespace BookingService.Application.Interfaces.Integrations
{
    public interface IPartnerCarReadClient
    {
        Task<PartnerCarPricingContext?> GetPricingContextAsync(
            int partnerCarId,
            DateTimeOffset startTime,
            DateTimeOffset endTime,
            CancellationToken cancellationToken = default);

        Task<PartnerCarSnapshotPayload?> GetSnapshotAsync(
            int partnerCarId,
            CancellationToken cancellationToken = default);
    }

    public sealed record PartnerCarPricingContext(
        int PartnerCarId,
        Guid PartnerUserId,
        int CarModelId,
        decimal? MarketValueKzt,
        decimal Rating,
        int CurrentAvailableCarsCount,
        bool IsMarketValueStale);

    public sealed class PartnerCarSnapshotPayload
    {
        public int PartnerCarId { get; set; }
        public Guid PartnerUserId { get; set; }
        public string CarBrand { get; set; } = string.Empty;
        public string CarModel { get; set; } = string.Empty;
        public int ModelYear { get; set; }
        public string? LicensePlate { get; set; }
        public string? Color { get; set; }
        public decimal? PriceHour { get; set; }
        public decimal? Rating { get; set; }
        public string? CoverImageUrl { get; set; }
        public IReadOnlyList<string> ImageUrls { get; set; } = [];
    }
}
