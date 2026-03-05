using CarService.Api.Extensions;
using CarService.Application.DTOs.CarImage;
using CarService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarService.Api.Controllers
{
    [ApiController]
    [Route("images")]
    public sealed class CarImagesController : ControllerBase
    {
        private readonly ICarImageService _carImageService;

        public CarImagesController(ICarImageService carImageService)
        {
            _carImageService = carImageService;
        }

        [HttpPost("models/{modelId:int}")]
        [Authorize(Policy = "car-models:update")]
        public async Task<IActionResult> CreateModelImage(
            int modelId,
            [FromBody] CarImageCreateRequestDto dto,
            CancellationToken cancellationToken)
        {
            var authorizationHeader = Request.Headers.Authorization.ToString();
            var created = await _carImageService.CreateModelImageAsync(modelId, dto, authorizationHeader, cancellationToken);
            return CreatedAtAction(nameof(GetModelImages), new { modelId }, created);
        }

        [HttpGet("models/{modelId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetModelImages(int modelId, CancellationToken cancellationToken)
        {
            return Ok(await _carImageService.GetModelImagesAsync(modelId, cancellationToken));
        }

        [HttpPut("models/{imageId:int}")]
        [Authorize(Policy = "car-models:update")]
        public async Task<IActionResult> UpdateModelImage(
            int imageId,
            [FromBody] CarImageUpdateDto dto,
            CancellationToken cancellationToken)
        {
            var authorizationHeader = Request.Headers.Authorization.ToString();
            var updated = await _carImageService.UpdateModelImageAsync(imageId, dto, authorizationHeader, cancellationToken);
            if (updated is null)
            {
                return NotFound(new { error = "Model image not found" });
            }

            return Ok(updated);
        }

        [HttpDelete("models/{imageId:int}")]
        [Authorize(Policy = "car-models:delete")]
        public async Task<IActionResult> DeleteModelImage(int imageId, CancellationToken cancellationToken)
        {
            var authorizationHeader = Request.Headers.Authorization.ToString();
            var deleted = await _carImageService.DeleteModelImageAsync(imageId, authorizationHeader, cancellationToken);
            if (!deleted)
            {
                return NotFound(new { error = "Model image not found" });
            }

            return NoContent();
        }

        [HttpPost("partner-cars/{partnerCarId:int}")]
        [Authorize(Policy = "car-images:create")]
        public async Task<IActionResult> CreatePartnerCarImage(
            int partnerCarId,
            [FromBody] CarImageCreateRequestDto dto,
            CancellationToken cancellationToken)
        {
            var currentUserId = User.GetRequiredUserGuid();
            var authorizationHeader = Request.Headers.Authorization.ToString();

            var created = await _carImageService.CreatePartnerCarImageAsync(
                currentUserId,
                partnerCarId,
                dto,
                authorizationHeader,
                cancellationToken);

            return CreatedAtAction(nameof(GetPartnerCarImages), new { partnerCarId }, created);
        }

        [HttpGet("partner-cars/{partnerCarId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPartnerCarImages(int partnerCarId, CancellationToken cancellationToken)
        {
            return Ok(await _carImageService.GetPartnerCarImagesAsync(partnerCarId, cancellationToken));
        }

        [HttpPut("partner-cars/{imageId:int}")]
        [Authorize(Policy = "car-images:update")]
        public async Task<IActionResult> UpdatePartnerCarImage(
            int imageId,
            [FromBody] CarImageUpdateDto dto,
            CancellationToken cancellationToken)
        {
            var currentUserId = User.GetRequiredUserGuid();
            var authorizationHeader = Request.Headers.Authorization.ToString();

            var updated = await _carImageService.UpdatePartnerCarImageAsync(
                currentUserId,
                imageId,
                dto,
                authorizationHeader,
                cancellationToken);

            if (updated is null)
            {
                return NotFound(new { error = "Partner car image not found" });
            }

            return Ok(updated);
        }

        [HttpDelete("partner-cars/{imageId:int}")]
        [Authorize(Policy = "car-images:delete")]
        public async Task<IActionResult> DeletePartnerCarImage(int imageId, CancellationToken cancellationToken)
        {
            var currentUserId = User.GetRequiredUserGuid();
            var authorizationHeader = Request.Headers.Authorization.ToString();

            var deleted = await _carImageService.DeletePartnerCarImageAsync(
                currentUserId,
                imageId,
                authorizationHeader,
                cancellationToken);

            if (!deleted)
            {
                return NotFound(new { error = "Partner car image not found" });
            }

            return NoContent();
        }
    }
}
