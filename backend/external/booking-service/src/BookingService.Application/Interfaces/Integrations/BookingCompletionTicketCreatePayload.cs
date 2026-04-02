namespace BookingService.Application.Interfaces.Integrations;

public sealed class BookingCompletionTicketCreatePayload
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public int BookingId { get; init; }
    public DateTimeOffset PlannedStartTime { get; init; }
    public DateTimeOffset PlannedEndTime { get; init; }
    public DateTimeOffset TripStartedAt { get; init; }
    public DateTimeOffset TripCompletedAt { get; init; }
    public decimal? LatePenaltyAmount { get; init; }
    public FileUploadPayload CompletionFrontPhotoFile { get; init; } = new();
    public FileUploadPayload CompletionBackPhotoFile { get; init; } = new();
    public FileUploadPayload CompletionSideLeftPhotoFile { get; init; } = new();
    public FileUploadPayload CompletionSideRightPhotoFile { get; init; } = new();
    public FileUploadPayload CompletionInteriorPhotoFile { get; init; } = new();
}
