using TicketService.Domain.Enums;

namespace TicketService.Application.Events;

public sealed record TicketRejectedEvent(
    Guid TicketId,
    TicketType TicketType,
    string FullName,
    string Email,
    string DecisionReason,
    Guid ManagerId);
