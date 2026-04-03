namespace BookingService.Application.DTOs.Booking
{
    public class BookingResponseDto
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public int PartnerCarId { get; set; }
        public Guid PartnerUserId { get; set; }
        public string CarBrand { get; set; } = string.Empty;
        public string CarModel { get; set; } = string.Empty;
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public decimal? PriceHour { get; set; }
        public decimal? TotalPrice { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? TripStartedAt { get; set; }
        public DateTimeOffset? TripCompletedAt { get; set; }
        public Guid? CompletionReviewTicketId { get; set; }
        public bool UsedSubscription { get; set; }
        public string? Status { get; set; }
    }
}
