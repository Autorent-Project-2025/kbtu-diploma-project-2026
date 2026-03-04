namespace CarService.Infrastructure.Options
{
    public sealed class BookingServiceOptions
    {
        public const string SectionName = "BookingService";

        public string BaseUrl { get; init; } = string.Empty;
        public string InternalApiKey { get; init; } = string.Empty;
    }
}
