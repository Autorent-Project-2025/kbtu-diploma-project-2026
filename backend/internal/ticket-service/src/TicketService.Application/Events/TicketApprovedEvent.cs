namespace TicketService.Application.Events;

public sealed record TicketApprovedEvent(
    Guid TicketId,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    DateOnly BirthDate,
    string PhoneNumber,
    string? IdentityDocumentFileName,
    string? DriverLicenseFileName,
    string? AvatarUrl,
    Guid ManagerId);
