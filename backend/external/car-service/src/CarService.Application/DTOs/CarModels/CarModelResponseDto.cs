namespace CarService.Application.DTOs.CarModels
{
    public class CarModelResponseDto
    {
        public int Id { get; set; }
        public string Brand { get; set; } = null!;
        public string Model { get; set; } = null!;
        public int Year { get; set; }
        public string? Engine { get; set; }
        public string? Transmission { get; set; }
        public int? Seats { get; set; }
        public string? FuelType { get; set; }
        public int? Doors { get; set; }
        public string? Description { get; set; }
        public decimal? Rating { get; set; }
        public int RatingsCount { get; set; }
    }
}
