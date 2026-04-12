namespace TicketService.Application.Complaints.Commands.RejectReopenRequest;

public sealed record RejectReopenRequestCommand(
    Guid ReopenRequestId,
    Guid ManagerId,
    string? Note);
