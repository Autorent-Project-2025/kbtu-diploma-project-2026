namespace TicketService.Application.Complaints.Commands.ApproveReopenRequest;

public sealed record ApproveReopenRequestCommand(
    Guid ReopenRequestId,
    Guid ManagerId,
    string? Note);
