using TicketService.Domain.Enums;

namespace TicketService.Application.Models;

public sealed record AccessRequestDto(
    Guid Id,
    Guid ComplaintId,
    int BookingId,
    Guid RequestedByManagerId,
    AccessRequestStatus Status,
    string Reason,
    DateTime RequestedAt,
    Guid? ReviewedBySupermanagerId,
    DateTime? ReviewedAt,
    string? DecisionNote,
    DateTime? ExpiresAt);
