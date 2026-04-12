namespace TicketService.Application.Commands.IssueTicketFine;

public sealed record IssueTicketFineCommand(Guid TicketId, Guid ManagerId, decimal Amount, string Comment);
