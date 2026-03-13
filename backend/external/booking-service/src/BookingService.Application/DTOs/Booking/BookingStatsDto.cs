namespace BookingService.Application.DTOs.Booking;

public sealed class BookingStatsDto
{
    public int TotalCount { get; set; }
    public int ActiveCount { get; set; }
    public int CompletedCount { get; set; }
    public decimal TotalSpent { get; set; }
}
