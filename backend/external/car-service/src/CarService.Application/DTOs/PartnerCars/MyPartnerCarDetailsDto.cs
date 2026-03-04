using CarService.Application.DTOs.Bookings;
using CarService.Application.DTOs.CarComment;
using CarService.Application.DTOs.CarImage;
using CarService.Domain.Enums;

namespace CarService.Application.DTOs.PartnerCars
{
    public class MyPartnerCarDetailsDto
    {
        public int Id { get; set; }
        public Guid PartnerId { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public string? Color { get; set; }
        public decimal? PriceHour { get; set; }
        public decimal? PriceDay { get; set; }
        public PartnerCarStatus Status { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public decimal? Rating { get; set; }
        public int RatingsCount { get; set; }

        public int ModelId { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public string? Engine { get; set; }
        public string? Transmission { get; set; }
        public int? Seats { get; set; }
        public string? FuelType { get; set; }
        public int? Doors { get; set; }
        public string? Description { get; set; }

        public List<CarImageDto> Images { get; set; } = [];
        public List<CarCommentResponseDto> Comments { get; set; } = [];
        public List<LinkedBookingDto> Bookings { get; set; } = [];
    }
}
