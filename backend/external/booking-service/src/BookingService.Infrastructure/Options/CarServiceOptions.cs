namespace BookingService.Infrastructure.Options
{
    public sealed class CarServiceOptions
    {
        public const string SectionName = "CarService";

        public string BaseUrl { get; set; } = string.Empty;
    }
}
