using TicketService.Application.Models;

namespace TicketService.Application.AccessRequests.Queries.GetAccessRequests;

public sealed record GetAccessRequestsResult(IReadOnlyCollection<AccessRequestDto> AccessRequests);
