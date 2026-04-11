namespace TicketService.Application.AccessRequests.Commands.RevokeAccessRequest;

public sealed record RevokeAccessRequestCommand(
    Guid RequestId,
    Guid SupermanagerId);
