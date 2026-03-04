namespace BookingService.Application.DTOs.Booking
{
    public class BookingResponseDto
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public int PartnerCarId { get; set; }
        public Guid PartnerId { get; set; }
        public string CarBrand { get; set; } = string.Empty;
        public string CarModel { get; set; } = string.Empty;
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public decimal? PriceHour { get; set; }
        public decimal? TotalPrice { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string? Status { get; set; }

        // Backward-compatible aliases for old clients.
        public int CarId => PartnerCarId;
        public DateTime StartDate => StartTime.UtcDateTime;
        public DateTime EndDate => EndTime.UtcDateTime;
        public decimal? Price => TotalPrice;
    }
}
