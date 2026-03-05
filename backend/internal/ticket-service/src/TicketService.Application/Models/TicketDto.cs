using TicketService.Domain.Enums;
using TicketService.Domain.Entities;

namespace TicketService.Application.Models;

public sealed record TicketDto(
    Guid Id,
    TicketType TicketType,
    TicketData Data,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    DateOnly? BirthDate,
    string PhoneNumber,
    string? IdentityDocumentFileName,
    string? DriverLicenseFileName,
    string? OwnershipDocumentFileName,
    string? AvatarUrl,
    Guid? RelatedPartnerUserId,
    string? CarBrand,
    string? CarModel,
    int? CarYear,
    string? LicensePlate,
    IReadOnlyCollection<PartnerCarTicketImageData> CarImages,
    TicketStatus Status,
    string? DecisionReason,
    DateTime CreatedAt,
    Guid? ReviewedByManagerId,
    DateTime? ReviewedAt);
