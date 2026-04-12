namespace BookingService.Application.DTOs;

public class PricePreviewQueryDto
{
    public Guid CarId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}