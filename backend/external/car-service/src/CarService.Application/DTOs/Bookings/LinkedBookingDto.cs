namespace CarService.Application.DTOs.Bookings
{
    public class LinkedBookingDto
    {
        public int Id { get; set; }
        public int CarId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal? Price { get; set; }
        public string? Status { get; set; }
    }
}
