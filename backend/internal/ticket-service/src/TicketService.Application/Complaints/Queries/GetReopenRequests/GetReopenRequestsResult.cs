using TicketService.Application.Models;

namespace TicketService.Application.Complaints.Queries.GetReopenRequests;

public sealed record GetReopenRequestsResult(
    IReadOnlyCollection<ReopenRequestDto> ReopenRequests);
