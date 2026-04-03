namespace CarService.Application.Interfaces
{
    public interface ICarMarketValueSyncService
    {
        Task EnsureCarModelMarketValueAsync(int carModelId, CancellationToken cancellationToken = default);
        Task RefreshCarModelMarketValueAsync(int carModelId, CancellationToken cancellationToken = default);
        Task RefreshStaleCarModelsAsync(CancellationToken cancellationToken = default);
    }
}
