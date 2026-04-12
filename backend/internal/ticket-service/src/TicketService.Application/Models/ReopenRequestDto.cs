using TicketService.Domain.Enums;

namespace TicketService.Application.Models;

public sealed record ReopenRequestDto(
    Guid Id,
    Guid ComplaintId,
    Guid RequestedByUserId,
    string Reason,
    ReopenRequestStatus Status,
    Guid? ReviewedByManagerId,
    DateTime? ReviewedAt,
    string? DecisionNote,
    DateTime CreatedAt);
