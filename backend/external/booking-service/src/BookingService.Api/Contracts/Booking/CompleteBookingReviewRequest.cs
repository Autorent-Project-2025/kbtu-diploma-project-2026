using Microsoft.AspNetCore.Http;

namespace BookingService.Api.Contracts.Booking;

public sealed class CompleteBookingReviewRequest
{
    public IFormFile? CompletionFrontPhotoFile { get; init; }
    public IFormFile? CompletionBackPhotoFile { get; init; }
    public IFormFile? CompletionSideLeftPhotoFile { get; init; }
    public IFormFile? CompletionSideRightPhotoFile { get; init; }
    public IFormFile? CompletionInteriorPhotoFile { get; init; }
}
