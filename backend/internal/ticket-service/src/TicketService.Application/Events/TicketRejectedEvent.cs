namespace TicketService.Application.Events;

public sealed record TicketRejectedEvent(
    Guid TicketId,
    string FullName,
    string Email,
    string DecisionReason,
    Guid ManagerId);
