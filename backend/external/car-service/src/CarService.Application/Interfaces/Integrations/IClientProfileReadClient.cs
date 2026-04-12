namespace CarService.Application.Interfaces.Integrations
{
    public interface IClientProfileReadClient
    {
        Task<string?> GetAvatarUrlByRelatedUserIdAsync(
            string relatedUserId,
            CancellationToken cancellationToken = default);
    }
}
