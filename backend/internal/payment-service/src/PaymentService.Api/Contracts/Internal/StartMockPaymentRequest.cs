namespace PaymentService.Api.Contracts.Internal;

public sealed class StartMockPaymentRequest
{
    public int BookingId { get; set; }
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "KZT";
}
