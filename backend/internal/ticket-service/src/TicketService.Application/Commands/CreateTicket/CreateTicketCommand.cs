using TicketService.Application.Models;

namespace TicketService.Application.Commands.CreateTicket;

public sealed record CreateTicketCommand(
    string FirstName,
    string LastName,
    string Email,
    DateOnly BirthDate,
    string PhoneNumber,
    string? AvatarUrl,
    TicketDocumentFilePayload IdentityDocumentFile,
    TicketDocumentFilePayload DriverLicenseFile);
