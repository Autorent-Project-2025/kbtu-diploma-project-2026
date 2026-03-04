namespace CarService.Application.DTOs.Matching
{
    public sealed class AvailableCarModelDto
    {
        public int ModelId { get; init; }
        public string Brand { get; init; } = string.Empty;
        public string Model { get; init; } = string.Empty;
        public int Year { get; init; }
        public int AvailableCarsCount { get; init; }
        public decimal? MinPriceHour { get; init; }
        public decimal? MaxPriceHour { get; init; }
        public decimal? AverageRating { get; init; }
    }
}
