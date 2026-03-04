using CarService.Application.DTOs.Matching;
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

        public CarMatchingController(IPartnerCarService partnerCarService)
        {
            _partnerCarService = partnerCarService;
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
        public async Task<IActionResult> Match([FromBody] MatchPartnerCarRequestDto request, CancellationToken cancellationToken)
        {
            var payload = await _partnerCarService.MatchPartnerCarAsync(request, cancellationToken);
            return Ok(payload);
        }
    }
}
