using TicketService.Application.Models;

namespace TicketService.Application.Commands.IssueTicketFine;

public sealed record IssueTicketFineResult(TicketDto Ticket);
