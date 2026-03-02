using TicketService.Application.Models;

namespace TicketService.Application.Commands.RejectTicket;

public sealed record RejectTicketResult(TicketDto Ticket);
