using TicketService.Application.Models;

namespace TicketService.Application.Complaints.Commands.RespondToInfoRequest;

public sealed record RespondToInfoRequestCommand(
    Guid ComplaintId,
    Guid ReporterUserId,
    string Message,
    IReadOnlyCollection<TicketDocumentFilePayload>? Attachments);
