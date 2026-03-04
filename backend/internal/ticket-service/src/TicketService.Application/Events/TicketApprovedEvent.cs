using TicketService.Domain.Entities;
using TicketService.Domain.Enums;

namespace TicketService.Application.Events;

public sealed record TicketApprovedEvent(
    Guid TicketId,
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
    Guid? RelatedPartnerUserId,
    string? CarBrand,
    string? CarModel,
    string? LicensePlate,
    string? OwnershipDocumentFileName,
    IReadOnlyCollection<PartnerCarTicketImageData> CarImages,
    Guid ManagerId,
    DateTime ReviewedAtUtc);
