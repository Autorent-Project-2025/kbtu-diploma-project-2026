using CarService.Api.Extensions;
using CarService.Application.DTOs.PartnerCars;
using CarService.Application.Interfaces;
using CarService.Application.Interfaces.Integrations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarService.Api.Controllers
{
    [ApiController]
    [Route("partner-cars")]
    public sealed class PartnerCarsController : ControllerBase
    {
        private readonly IPartnerCarService _partnerCarService;
        private readonly IPartnerContextClient _partnerContextClient;

        public PartnerCarsController(
            IPartnerCarService partnerCarService,
            IPartnerContextClient partnerContextClient)
        {
            _partnerCarService = partnerCarService;
            _partnerContextClient = partnerContextClient;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] PartnerCarQueryParams queryParams, CancellationToken cancellationToken)
        {
            var payload = await _partnerCarService.GetAllAsync(queryParams, cancellationToken);
            return Ok(payload);
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var payload = await _partnerCarService.GetByIdAsync(id, cancellationToken);
            if (payload is null)
            {
                return NotFound(new { error = "Partner car not found" });
            }

            return Ok(payload);
        }

        [HttpPost]
        [Authorize(Policy = "partner-cars:create")]
        public async Task<IActionResult> Create([FromBody] PartnerCarCreateDto dto, CancellationToken cancellationToken)
        {
            var currentUserId = User.GetRequiredUserGuid();
            if (!await IsCurrentUserPartnerAsync(currentUserId, cancellationToken))
            {
                return Forbid();
            }

            var created = await _partnerCarService.CreateAsync(currentUserId, dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        [Authorize(Policy = "partner-cars:update")]
        public async Task<IActionResult> Update(int id, [FromBody] PartnerCarUpdateDto dto, CancellationToken cancellationToken)
        {
            var currentUserId = User.GetRequiredUserGuid();
            if (!await IsCurrentUserPartnerAsync(currentUserId, cancellationToken))
            {
                return Forbid();
            }

            var updated = await _partnerCarService.UpdateAsync(currentUserId, id, dto, cancellationToken);
            if (updated is null)
            {
                return NotFound(new { error = "Partner car not found" });
            }

            return Ok(updated);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = "partner-cars:delete")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var currentUserId = User.GetRequiredUserGuid();
            if (!await IsCurrentUserPartnerAsync(currentUserId, cancellationToken))
            {
                return Forbid();
            }

            var deleted = await _partnerCarService.DeleteAsync(currentUserId, id, cancellationToken);
            if (!deleted)
            {
                return NotFound(new { error = "Partner car not found" });
            }

            return NoContent();
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
