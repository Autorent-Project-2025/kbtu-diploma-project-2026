namespace TicketService.Application.Complaints.Commands.CreateReopenRequest;

public sealed record CreateReopenRequestCommand(
    Guid ComplaintId,
    Guid RequestedByUserId,
    string Reason);
