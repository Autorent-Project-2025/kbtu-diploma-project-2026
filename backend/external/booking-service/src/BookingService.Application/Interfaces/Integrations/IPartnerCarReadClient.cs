namespace BookingService.Application.Interfaces.Integrations
{
    public interface IPartnerCarReadClient
    {
        Task<PartnerCarPricingContext?> GetPricingContextAsync(
            int partnerCarId,
            DateTimeOffset startTime,
            DateTimeOffset endTime,
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
}
