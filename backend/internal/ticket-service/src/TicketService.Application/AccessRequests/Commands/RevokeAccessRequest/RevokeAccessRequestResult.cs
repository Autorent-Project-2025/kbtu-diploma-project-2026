using TicketService.Application.Models;

namespace TicketService.Application.AccessRequests.Commands.RevokeAccessRequest;

public sealed record RevokeAccessRequestResult(AccessRequestDto AccessRequest);
