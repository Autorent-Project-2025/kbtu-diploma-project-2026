namespace BookingService.Api.Contracts.Internal;

public sealed class ApproveBookingCompletionReviewRequest
{
    public Guid TicketId { get; init; }
    public decimal? LatePenaltyAmount { get; init; }
    public string CustomerEmail { get; init; } = string.Empty;
    public string CustomerFullName { get; init; } = string.Empty;
}
