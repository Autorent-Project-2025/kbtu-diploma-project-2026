namespace TicketService.Application.AccessRequests.Commands.CreateAccessRequest;

public sealed record CreateAccessRequestCommand(
    Guid ComplaintId,
    Guid ManagerId,
    string Reason);
