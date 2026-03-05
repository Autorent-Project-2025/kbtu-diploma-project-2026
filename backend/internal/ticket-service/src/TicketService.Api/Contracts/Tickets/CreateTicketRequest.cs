using Microsoft.AspNetCore.Http;

namespace TicketService.Api.Contracts.Tickets;

public sealed class CreateTicketRequest
{
    public string? TicketType { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? CompanyName { get; init; }
    public string? ContactEmail { get; init; }
    public string? Email { get; init; }
    public DateOnly? BirthDate { get; init; }
    public string? PhoneNumber { get; init; }
    public string? AvatarUrl { get; init; }
    public IFormFile? IdentityDocumentFile { get; init; }
    public IFormFile? DriverLicenseFile { get; init; }
    public string? CarBrand { get; init; }
    public string? CarModel { get; init; }
    public int? CarYear { get; init; }
    public string? LicensePlate { get; init; }
    public decimal? PriceHour { get; init; }
    public decimal? PriceDay { get; init; }
    public IFormFile? OwnershipDocumentFile { get; init; }
    public List<IFormFile>? CarImageFiles { get; init; }
}
