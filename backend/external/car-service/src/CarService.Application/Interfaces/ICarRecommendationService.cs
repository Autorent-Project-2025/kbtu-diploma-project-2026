using CarService.Application.DTOs.Recommendation;

namespace CarService.Application.Interfaces;

public interface ICarRecommendationService
{
    Task<List<RecommendedPartnerCarDto>> GetRecommendationsAsync(
        RecommendationQueryDto query,
        CancellationToken cancellationToken = default);
}