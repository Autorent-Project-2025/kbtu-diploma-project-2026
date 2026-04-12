namespace TicketService.Application.AccessRequests.Commands.ApproveAccessRequest;

public sealed record ApproveAccessRequestCommand(
    Guid RequestId,
    Guid SupermanagerId,
    string? DecisionNote,
    int ExpiresInHours = 24);
