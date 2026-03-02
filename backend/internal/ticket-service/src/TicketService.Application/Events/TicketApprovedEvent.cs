namespace TicketService.Application.Events;

public sealed record TicketApprovedEvent(
    Guid TicketId,
    string FullName,
    string Email,
    DateOnly BirthDate,
    Guid ManagerId);
