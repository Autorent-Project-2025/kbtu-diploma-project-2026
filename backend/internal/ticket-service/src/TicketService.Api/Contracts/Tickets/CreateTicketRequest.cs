namespace TicketService.Api.Contracts.Tickets;

public sealed class CreateTicketRequest
{
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateOnly BirthDate { get; init; }
    public string PhoneNumber { get; init; } = string.Empty;
    public string? IdentityDocumentFileName { get; init; }
    public string? DriverLicenseFileName { get; init; }
    public string? AvatarUrl { get; init; }
}
