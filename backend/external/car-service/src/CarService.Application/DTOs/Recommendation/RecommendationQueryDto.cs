namespace CarService.Application.DTOs.Recommendation;

public class RecommendationQueryDto
{
    public decimal? MaxBudgetPerHour { get; set; }
    public int? Passengers { get; set; }
    public string? TripPurpose { get; set; }
    public string? Transmission { get; set; }
}