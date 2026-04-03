namespace CarService.Infrastructure.Options
{
    public sealed class CarMarketValueServiceOptions
    {
        public const string SectionName = "CarMarketValueService";

        public string BaseUrl { get; init; } = string.Empty;
    }
}
