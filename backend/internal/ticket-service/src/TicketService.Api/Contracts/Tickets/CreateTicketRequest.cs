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
    public string? PartnerCarRequestKind { get; init; }
    public int? PartnerCarId { get; init; }
    public string? LicensePlate { get; init; }
    public string? Color { get; init; }
    public int? RequestedStatus { get; init; }
    public bool? IsActive { get; init; }
    public string? Transmission { get; init; }
    public string? FuelType { get; init; }
    public int? Seats { get; init; }
    public int? Doors { get; init; }
    public string? BodyType { get; init; }
    public int? Horsepower { get; init; }
    public List<string>? SelectedTags { get; init; }
    public IFormFile? OwnershipDocumentFile { get; init; }
    public List<IFormFile>? CarImageFiles { get; init; }
    public List<string>? CarImageTypes { get; init; }
    public Guid? RelatedPartnerUserId { get; init; }
    public int? BookingId { get; init; }
    public string? BookingStatus { get; init; }
    public DateTimeOffset? BookingStartTime { get; init; }
    public DateTimeOffset? BookingEndTime { get; init; }
    public string? PartnerReason { get; init; }
    public DateTimeOffset? PlannedStartTime { get; init; }
    public DateTimeOffset? PlannedEndTime { get; init; }
    public DateTimeOffset? TripStartedAt { get; init; }
    public DateTimeOffset? TripCompletedAt { get; init; }
    public decimal? LatePenaltyAmount { get; init; }
    public IFormFile? CompletionFrontPhotoFile { get; init; }
    public IFormFile? CompletionBackPhotoFile { get; init; }
    public IFormFile? CompletionSideLeftPhotoFile { get; init; }
    public IFormFile? CompletionSideRightPhotoFile { get; init; }
    public IFormFile? CompletionInteriorPhotoFile { get; init; }

    // JSON-serialised BookingCompletionAiAssessmentData produced by
    // booking-service. Transported as a single form field to keep the
    // public contract stable across AI schema changes.
    public string? DamageAssessment { get; init; }
}
