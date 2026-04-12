namespace CarService.Application.Interfaces
{
    public interface IPartnerCarDisplayPricingService
    {
        Task RecalculateForCarModelAsync(int carModelId, CancellationToken cancellationToken = default);
        Task RecalculateForPartnerCarAsync(int partnerCarId, CancellationToken cancellationToken = default);
    }
}
