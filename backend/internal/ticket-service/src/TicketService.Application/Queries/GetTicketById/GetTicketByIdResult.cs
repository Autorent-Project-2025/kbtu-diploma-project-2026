using TicketService.Application.Models;

namespace TicketService.Application.Queries.GetTicketById;

public sealed record GetTicketByIdResult(TicketDto Ticket);
