namespace ChatService.Application.Interfaces;

public interface IFileServiceClient
{
    Task<FileUploadResult> UploadFileAsync(
        Stream fileStream, string fileName, string contentType, CancellationToken ct = default);

    Task<string> GetTemporaryLinkAsync(
        string storedFileName, int ttlSeconds = 900, CancellationToken ct = default);
}

public sealed record FileUploadResult(string FileName, string OriginalFileName);
