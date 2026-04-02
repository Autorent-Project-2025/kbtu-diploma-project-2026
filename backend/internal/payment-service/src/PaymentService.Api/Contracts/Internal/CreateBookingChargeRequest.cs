namespace PaymentService.Api.Contracts.Internal;

public sealed class CreateBookingChargeRequest
{
    public int BookingId { get; init; }
    public Guid UserId { get; init; }
    public Guid PartnerUserId { get; init; }
    public string ChargeType { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string? Currency { get; init; }
    public string? Description { get; init; }
}
