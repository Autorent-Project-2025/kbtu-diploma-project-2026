namespace ClientService.Application.Interfaces.Integrations;

public interface IImageStorageClient
{
    Task DeleteAsync(
        string imageId,
        string authorizationHeader,
        CancellationToken cancellationToken = default);
}
