using TicketService.Application.Models;
using TicketService.Domain.Enums;

namespace TicketService.Application.Commands.CreateTicket;

public sealed record CreateTicketCommand(
    string? AuthorizationHeader,
    TicketType TicketType,
    string FirstName,
    string LastName,
    string? CompanyName,
    string? ContactEmail,
    string Email,
    DateOnly? BirthDate,
    string PhoneNumber,
    string? AvatarUrl,
    TicketDocumentFilePayload? IdentityDocumentFile,
    TicketDocumentFilePayload? DriverLicenseFile,
    string? CarBrand,
    string? CarModel,
    int? CarYear,
    string? LicensePlate,
    decimal? PriceHour,
    decimal? PriceDay,
    TicketDocumentFilePayload? OwnershipDocumentFile,
    IReadOnlyCollection<TicketDocumentFilePayload>? CarImageFiles);
