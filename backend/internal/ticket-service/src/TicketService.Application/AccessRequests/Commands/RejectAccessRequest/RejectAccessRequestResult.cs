using TicketService.Application.Models;

namespace TicketService.Application.AccessRequests.Commands.RejectAccessRequest;

public sealed record RejectAccessRequestResult(AccessRequestDto AccessRequest);
