using TicketService.Application.Models;

namespace TicketService.Application.AccessRequests.Queries.GetAccessRequestById;

public sealed record GetAccessRequestByIdResult(AccessRequestDto AccessRequest);
