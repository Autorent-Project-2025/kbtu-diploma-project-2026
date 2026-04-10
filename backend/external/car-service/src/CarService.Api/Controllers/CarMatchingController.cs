using CarService.Application.DTOs.Matching;
using CarService.Application.DTOs.Recommendation;
using CarService.Application.Interfaces;
using CarService.Application.Interfaces.Integrations;
using CarService.Domain.Calculations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarService.Api.Controllers
{
    [ApiController]
    [Route("")]
    public sealed class CarMatchingController : ControllerBase
    {
        private readonly IPartnerCarService _partnerCarService;
        private readonly ICarRecommendationService _carRecommendationService;
        private readonly ICarMarketValueClient _marketValueClient;

        public CarMatchingController(
            IPartnerCarService partnerCarService,
            ICarRecommendationService carRecommendationService,
            ICarMarketValueClient marketValueClient)
        {
            _partnerCarService = partnerCarService;
            _carRecommendationService = carRecommendationService;
            _marketValueClient = marketValueClient;
        }

        [HttpGet("available-models")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailableModels(CancellationToken cancellationToken)
        {
            var payload = await _partnerCarService.GetAvailableModelsAsync(cancellationToken);
            return Ok(payload);
        }

        [HttpPost("match")]
        [AllowAnonymous]
        public async Task<IActionResult> Match(
            [FromBody] MatchPartnerCarRequestDto request,
            CancellationToken cancellationToken)
        {
            var payload = await _partnerCarService.MatchPartnerCarAsync(request, cancellationToken);
            return Ok(payload);
        }

        [HttpGet("price-estimate")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPriceEstimate(
            [FromQuery] string brand,
            [FromQuery] string model,
            [FromQuery] int year,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(brand) || string.IsNullOrWhiteSpace(model) || year < 1886)
                return BadRequest("brand, model and year are required.");

            try
            {
                var estimate = await _marketValueClient.GetMarketValueAsync(brand, model, year, cancellationToken);
                var (priceHour, priceDay) = PartnerCarDisplayPriceCalculator.Calculate(estimate.MarketValueKzt, null);

                return Ok(new PriceEstimateResponseDto
                {
                    MarketValueKzt = estimate.MarketValueKzt,
                    PriceHour = priceHour ?? 0m,
                    PriceDay = priceDay ?? 0m,
                    Confidence = estimate.Confidence,
                    SampleCount = estimate.SampleCount,
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Comparable listings not found for the specified car.");
            }
        }

        [HttpGet("recommendations")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRecommendations(
            [FromQuery] RecommendationQueryDto query,
            CancellationToken cancellationToken)
        {
            var payload = await _carRecommendationService.GetRecommendationsAsync(query, cancellationToken);
            return Ok(payload);
        }
    }
}