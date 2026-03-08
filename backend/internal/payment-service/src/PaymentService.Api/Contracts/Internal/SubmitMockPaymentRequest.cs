namespace PaymentService.Api.Contracts.Internal;

public sealed class SubmitMockPaymentRequest
{
    public Guid UserId { get; set; }
    public string SessionKey { get; set; } = string.Empty;
    public string CardHolder { get; set; } = string.Empty;
    public string CardNumber { get; set; } = string.Empty;
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public string Cvv { get; set; } = string.Empty;
}
