using TicketService.Application.Models;

namespace TicketService.Application.Queries.GetAllTickets;

public sealed record GetAllTicketsResult(TicketDto[] Tickets);
