namespace BookingService.Infrastructure.Options
{
    public sealed class PartnerServiceOptions
    {
        public const string SectionName = "PartnerService";

        public string BaseUrl { get; set; } = string.Empty;
        public string InternalApiKey { get; set; } = string.Empty;
    }
}
