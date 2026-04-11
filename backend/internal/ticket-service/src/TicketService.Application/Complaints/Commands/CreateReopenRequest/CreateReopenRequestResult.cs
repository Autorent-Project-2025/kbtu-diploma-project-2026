using TicketService.Application.Models;

namespace TicketService.Application.Complaints.Commands.CreateReopenRequest;

public sealed record CreateReopenRequestResult(ReopenRequestDto ReopenRequest);
