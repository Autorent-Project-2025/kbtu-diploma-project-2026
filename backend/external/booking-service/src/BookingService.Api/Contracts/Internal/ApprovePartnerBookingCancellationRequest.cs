namespace BookingService.Api.Contracts.Internal;

public sealed class ApprovePartnerBookingCancellationRequest
{
    public Guid TicketId { get; init; }
    public string PartnerReason { get; init; } = string.Empty;
}
