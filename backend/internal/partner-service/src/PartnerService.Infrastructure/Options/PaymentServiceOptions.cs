namespace PartnerService.Infrastructure.Options;

public sealed class PaymentServiceOptions
{
    public const string SectionName = "PaymentService";

    public string BaseUrl { get; init; } = string.Empty;
    public string InternalApiKey { get; init; } = string.Empty;
}
