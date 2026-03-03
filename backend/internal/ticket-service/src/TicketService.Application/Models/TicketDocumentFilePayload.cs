namespace TicketService.Application.Models;

public sealed record TicketDocumentFilePayload(
    string FileName,
    string ContentType,
    byte[] Content);
