using TicketService.Domain.Enums;

namespace TicketService.Application.AccessRequests.Queries.GetAccessRequests;

public sealed record GetAccessRequestsQuery(AccessRequestStatus? Status);
