namespace CarService.Application.DTOs.Cars
{
    public class CarResponseDto
    {
        public int Id { get; set; }
        public string Brand { get; set; } = null!;
        public string Model { get; set; } = null!;
        public int Year { get; set; }
        public decimal? PriceHour { get; set; }
        public decimal? PriceDay { get; set; }
        public string? ImageUrl { get; set; }
        public decimal? Rating { get; set; }
    }
}
