namespace CarService.Application.DTOs.Recommendation;

public class RecommendedPartnerCarDto
{
    public int PartnerCarId { get; set; }
    public int CarModelId { get; set; }

    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }

    public decimal? PriceHour { get; set; }
    public decimal? PriceDay { get; set; }

    public int? Seats { get; set; }
    public string? Transmission { get; set; }
    public decimal? Rating { get; set; }

    public decimal Score { get; set; }
    public string ReasonTag { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}