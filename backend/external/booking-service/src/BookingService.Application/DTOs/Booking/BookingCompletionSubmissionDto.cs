using BookingService.Application.Interfaces.Integrations;

namespace BookingService.Application.DTOs.Booking;

public sealed class BookingCompletionSubmissionDto
{
    public FileUploadPayload CompletionFrontPhotoFile { get; init; } = new();
    public FileUploadPayload CompletionBackPhotoFile { get; init; } = new();
    public FileUploadPayload CompletionSideLeftPhotoFile { get; init; } = new();
    public FileUploadPayload CompletionSideRightPhotoFile { get; init; } = new();
    public FileUploadPayload CompletionInteriorPhotoFile { get; init; } = new();
}
