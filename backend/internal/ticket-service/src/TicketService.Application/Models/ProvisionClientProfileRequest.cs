namespace TicketService.Application.Models;

public sealed record ProvisionClientProfileRequest(
    string FirstName,
    string LastName,
    DateOnly BirthDate,
    string? IdentityDocumentFileName,
    string? DriverLicenseFileName,
    Guid RelatedUserId,
    string PhoneNumber,
    string? AvatarUrl);
