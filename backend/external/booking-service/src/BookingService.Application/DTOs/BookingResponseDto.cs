namespace BookingService.Application.DTOs.Booking
{
    public class BookingResponseDto
    {
        public int Id { get; set; }
        public int CarId { get; set; }
        public string CarBrand { get; set; } = null!;
        public string CarModel { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal? Price { get; set; }
        public string? Status { get; set; }
    }
}
