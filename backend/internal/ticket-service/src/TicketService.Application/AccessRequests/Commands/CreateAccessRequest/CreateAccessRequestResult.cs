using TicketService.Application.Models;

namespace TicketService.Application.AccessRequests.Commands.CreateAccessRequest;

public sealed record CreateAccessRequestResult(AccessRequestDto AccessRequest);
