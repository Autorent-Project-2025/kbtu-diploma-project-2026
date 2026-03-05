using CarService.Api.Extensions;
using CarService.Application.Interfaces;
using CarService.Application.Interfaces.Integrations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarService.Api.Controllers
{
    [ApiController]
    [Route("my")]
    [Authorize(Policy = "partner-cars:view-own")]
    public sealed class MyCarsController : ControllerBase
    {
        private readonly IPartnerCarService _partnerCarService;
        private readonly IPartnerContextClient _partnerContextClient;

        public MyCarsController(
            IPartnerCarService partnerCarService,
            IPartnerContextClient partnerContextClient)
        {
            _partnerCarService = partnerCarService;
            _partnerContextClient = partnerContextClient;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyCars(CancellationToken cancellationToken)
        {
            var currentUserId = User.GetRequiredUserGuid();
            if (!await IsCurrentUserPartnerAsync(currentUserId, cancellationToken))
            {
                return Forbid();
            }

            var payload = await _partnerCarService.GetMyCarsAsync(currentUserId, cancellationToken);
            return Ok(payload);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetMyCarDetails(int id, CancellationToken cancellationToken)
        {
            var currentUserId = User.GetRequiredUserGuid();
            if (!await IsCurrentUserPartnerAsync(currentUserId, cancellationToken))
            {
                return Forbid();
            }

            var payload = await _partnerCarService.GetMyCarDetailsAsync(currentUserId, id, cancellationToken);
            if (payload is null)
            {
                return NotFound(new { error = "Partner car not found" });
            }

            return Ok(payload);
        }

        private async Task<bool> IsCurrentUserPartnerAsync(Guid currentUserId, CancellationToken cancellationToken)
        {
            var authorizationHeader = Request.Headers.Authorization.ToString();
            if (string.IsNullOrWhiteSpace(authorizationHeader))
            {
                return false;
            }

            var partner = await _partnerContextClient.GetCurrentPartnerAsync(authorizationHeader, cancellationToken);
            if (partner is null)
            {
                return false;
            }

            return Guid.TryParse(partner.RelatedUserId, out var relatedUserId)
                && relatedUserId == currentUserId;
        }
    }
}
