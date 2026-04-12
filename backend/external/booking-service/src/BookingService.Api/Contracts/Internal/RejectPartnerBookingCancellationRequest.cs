namespace BookingService.Api.Contracts.Internal;

public sealed class RejectPartnerBookingCancellationRequest
{
    public Guid TicketId { get; init; }
    public string DecisionReason { get; init; } = string.Empty;
}
