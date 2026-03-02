using TicketService.Application.Models;

namespace TicketService.Application.Commands.ApproveTicket;

public sealed record ApproveTicketResult(TicketDto Ticket);
