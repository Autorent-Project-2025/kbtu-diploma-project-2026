using TicketService.Application.Models;

namespace TicketService.Application.Complaints.Commands.RejectReopenRequest;

public sealed record RejectReopenRequestResult(ReopenRequestDto ReopenRequest);
