using Microsoft.AspNetCore.Http;

namespace TicketService.Api.Contracts.Tickets;

public sealed class CreateTicketRequest
{
    public string? TicketType { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateOnly? BirthDate { get; init; }
    public string PhoneNumber { get; init; } = string.Empty;
    public string? AvatarUrl { get; init; }
    public IFormFile? IdentityDocumentFile { get; init; }
    public IFormFile? DriverLicenseFile { get; init; }
}
