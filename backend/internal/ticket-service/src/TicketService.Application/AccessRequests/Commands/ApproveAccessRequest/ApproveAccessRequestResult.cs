using TicketService.Application.Models;

namespace TicketService.Application.AccessRequests.Commands.ApproveAccessRequest;

public sealed record ApproveAccessRequestResult(AccessRequestDto AccessRequest);
