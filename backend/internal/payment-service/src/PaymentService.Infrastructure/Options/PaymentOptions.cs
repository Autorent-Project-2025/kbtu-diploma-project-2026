namespace PaymentService.Infrastructure.Options;

public sealed class PaymentOptions
{
    public const string SectionName = "Payment";

    public string Currency { get; set; } = "KZT";
    public decimal PlatformCommissionRate { get; set; } = 0.10m;
}
