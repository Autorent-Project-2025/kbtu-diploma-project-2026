using TicketService.Application.Models;

namespace TicketService.Application.Interfaces;

public interface IImageStorageClient
{
    Task<ImageStorageUploadResult> UploadAsync(
        TicketDocumentFilePayload payload,
        string authorizationHeader,
        CancellationToken cancellationToken = default);
}
