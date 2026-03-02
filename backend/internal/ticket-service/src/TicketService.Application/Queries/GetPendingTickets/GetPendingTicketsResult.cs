using TicketService.Application.Models;

namespace TicketService.Application.Queries.GetPendingTickets;

public sealed record GetPendingTicketsResult(IReadOnlyCollection<TicketDto> Tickets);
