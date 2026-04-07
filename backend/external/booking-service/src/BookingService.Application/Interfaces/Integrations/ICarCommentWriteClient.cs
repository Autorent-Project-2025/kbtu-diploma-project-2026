namespace BookingService.Application.Interfaces.Integrations;

public interface ICarCommentWriteClient
{
    Task<CreatedCarCommentPayload> CreateForCompletedBookingAsync(
        CreateCompletedBookingCarCommentPayload payload,
        CancellationToken cancellationToken = default);
}
