namespace BookingService.Infrastructure.Options
{
    public sealed class PaymentServiceOptions
    {
        public const string SectionName = "PaymentService";

        public string BaseUrl { get; set; } = string.Empty;
        public string InternalApiKey { get; set; } = string.Empty;
        public string Currency { get; set; } = "KZT";
    }
}
