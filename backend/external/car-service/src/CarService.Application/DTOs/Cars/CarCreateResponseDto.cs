using CarService.Application.DTOs.CarFeature;

namespace CarService.Application.DTOs.Cars
{
    public class CarCreateResponseDto
    {
        public int Id { get; set; }
        public string Brand { get; set; } = null!;
        public string Model { get; set; } = null!;
        public int Year { get; set; }
        public decimal? PriceHour { get; set; }
        public decimal? PriceDay { get; set; }
        public string? ImageUrl { get; set; }
        public decimal? Rating { get; set; }
        public string? Description { get; set; }
        public string? Engine { get; set; }
        public string? Transmission { get; set; }
        public int? Seats { get; set; }
        public string? FuelType { get; set; }
        public string? Color { get; set; }
        public int? Doors { get; set; }

        public List<CarFeatureDto> Features { get; set; } = [];
    }
}
