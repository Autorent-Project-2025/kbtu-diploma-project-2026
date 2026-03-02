using TicketService.Domain.Enums;

namespace TicketService.Application.Models;

public sealed record TicketDto(
    Guid Id,
    string FullName,
    string Email,
    DateOnly BirthDate,
    TicketStatus Status,
    string? DecisionReason,
    DateTime CreatedAt,
    Guid? ReviewedByManagerId,
    DateTime? ReviewedAt);
