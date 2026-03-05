using TicketService.Domain.Enums;

namespace TicketService.Application.Events;

public sealed record TicketRejectedEvent(
    Guid TicketId,
    TicketType TicketType,
    string FullName,
    string Email,
    string? CarBrand,
    string? CarModel,
    string? LicensePlate,
    string DecisionReason,
    Guid ManagerId);
