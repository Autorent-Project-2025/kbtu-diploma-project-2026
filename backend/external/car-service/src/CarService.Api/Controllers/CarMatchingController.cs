using CarService.Application.DTOs.Matching;
using CarService.Application.DTOs.Recommendation;
using CarService.Application.Interfaces;
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

        public CarMatchingController(
            IPartnerCarService partnerCarService,
            ICarRecommendationService carRecommendationService)
        {
            _partnerCarService = partnerCarService;
            _carRecommendationService = carRecommendationService;
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