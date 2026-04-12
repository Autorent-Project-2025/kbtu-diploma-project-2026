namespace TicketService.Application.AccessRequests.Commands.RejectAccessRequest;

public sealed record RejectAccessRequestCommand(
    Guid RequestId,
    Guid SupermanagerId,
    string? DecisionNote);
