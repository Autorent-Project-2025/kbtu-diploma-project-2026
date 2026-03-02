using TicketService.Application.Models;

namespace TicketService.Application.Commands.CreateTicket;

public sealed record CreateTicketResult(TicketDto Ticket);
