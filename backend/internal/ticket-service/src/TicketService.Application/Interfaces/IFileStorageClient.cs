using TicketService.Application.Models;

namespace TicketService.Application.Interfaces;

public interface IFileStorageClient
{
    Task<string> UploadFileAsync(
        TicketDocumentFilePayload payload,
        CancellationToken cancellationToken = default);

    Task<FileTemporaryLinkResult> GetTemporaryLinkAsync(
        string fileName,
        int? ttlSeconds = null,
        CancellationToken cancellationToken = default);
}
