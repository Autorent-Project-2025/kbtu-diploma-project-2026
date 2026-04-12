namespace TicketService.Application.Complaints.Commands.RequestInfo;

public sealed record RequestInfoCommand(Guid ComplaintId, Guid ManagerId, string Message);
