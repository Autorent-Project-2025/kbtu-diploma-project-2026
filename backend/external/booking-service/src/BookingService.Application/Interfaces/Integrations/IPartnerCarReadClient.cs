namespace BookingService.Application.Interfaces.Integrations
{
    public interface IPartnerCarReadClient
    {
        Task<PartnerCarSnapshot?> GetByIdAsync(int partnerCarId, CancellationToken cancellationToken = default);
    }

    public sealed record PartnerCarSnapshot(Guid PartnerUserId, decimal? PriceHour);
}
