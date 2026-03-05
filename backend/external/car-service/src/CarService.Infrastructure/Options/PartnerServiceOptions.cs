namespace CarService.Infrastructure.Options
{
    public sealed class PartnerServiceOptions
    {
        public const string SectionName = "PartnerService";

        public string BaseUrl { get; init; } = string.Empty;
    }
}
