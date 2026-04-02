namespace BookingService.Application.DTOs;

public class PricePreviewDto
{
    public int PartnerCarId { get; set; }
    public decimal BasePricePerHour { get; set; }
    public int TotalHours { get; set; }
    public int Days { get; set; }
    public decimal DemandCoefficient { get; set; }
    public decimal WeekendCoefficient { get; set; }
    public decimal DurationCoefficient { get; set; }
    public decimal FinalPrice { get; set; }
    public string Currency { get; set; } = "AED";
    public string DemandLevel { get; set; } = "Low";
    public string Explanation { get; set; } = string.Empty;
}