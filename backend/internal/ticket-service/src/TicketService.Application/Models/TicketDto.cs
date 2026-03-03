using TicketService.Domain.Enums;

namespace TicketService.Application.Models;

public sealed record TicketDto(
    Guid Id,
    TicketType TicketType,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    DateOnly? BirthDate,
    string PhoneNumber,
    string? IdentityDocumentFileName,
    string? DriverLicenseFileName,
    string? AvatarUrl,
    TicketStatus Status,
    string? DecisionReason,
    DateTime CreatedAt,
    Guid? ReviewedByManagerId,
    DateTime? ReviewedAt);
