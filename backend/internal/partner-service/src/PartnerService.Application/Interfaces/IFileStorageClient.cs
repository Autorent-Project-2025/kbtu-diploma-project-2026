using PartnerService.Application.Models;

namespace PartnerService.Application.Interfaces;

public interface IFileStorageClient
{
    Task<FileTemporaryLinkResult> GetTemporaryLinkAsync(
        string fileName,
        int? ttlSeconds = null,
        CancellationToken cancellationToken = default);
}
