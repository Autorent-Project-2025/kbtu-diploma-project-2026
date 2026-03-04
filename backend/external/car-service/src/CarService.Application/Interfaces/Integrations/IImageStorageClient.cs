namespace CarService.Application.Interfaces.Integrations
{
    public interface IImageStorageClient
    {
        Task<ImageStorageUploadResult> UploadAsync(
            byte[] content,
            string authorizationHeader,
            CancellationToken cancellationToken = default);

        Task DeleteAsync(
            string imageId,
            string authorizationHeader,
            CancellationToken cancellationToken = default);
    }

    public sealed record ImageStorageUploadResult(string ImageId, string ImageUrl);
}
