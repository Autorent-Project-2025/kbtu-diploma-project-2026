using TicketService.Domain.Enums;

namespace TicketService.Application.Events;

public sealed record TicketFineIssuedEvent(
    Guid TicketId,
    TicketType TicketType,
    decimal Amount,
    Guid ManagerId,
    DateTime ReviewedAtUtc);
