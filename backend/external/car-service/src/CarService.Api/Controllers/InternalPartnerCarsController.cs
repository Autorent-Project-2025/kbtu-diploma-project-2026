using System.Security.Cryptography;
using System.Text;
using CarService.Api.Contracts.Internal;
using CarService.Api.Options;
using CarService.Application.DTOs.PartnerCars;
using CarService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CarService.Api.Controllers
{
    [ApiController]
    [Route("internal/partner-cars")]
    public sealed class InternalPartnerCarsController : ControllerBase
    {
        private const string InternalApiKeyHeader = "X-Internal-Api-Key";

        private readonly IPartnerCarService _partnerCarService;
        private readonly InternalAuthOptions _internalAuthOptions;

        public InternalPartnerCarsController(
            IPartnerCarService partnerCarService,
            IOptions<InternalAuthOptions> internalAuthOptions)
        {
            _partnerCarService = partnerCarService;
            _internalAuthOptions = internalAuthOptions.Value;
        }

        [AllowAnonymous]
        [HttpPost("provision")]
        public async Task<IActionResult> Provision(
            [FromBody] ProvisionPartnerCarRequest request,
            CancellationToken cancellationToken)
        {
            if (!IsAuthorizedInternalRequest())
            {
                return Unauthorized(new { error = "Internal API key is invalid." });
            }

            if (request.RelatedUserId == Guid.Empty)
            {
                return BadRequest(new { error = "RelatedUserId is required." });
            }

            var maxAllowedCarYear = DateTime.UtcNow.Year + 1;
            if (request.CarYear < 1886 || request.CarYear > maxAllowedCarYear)
            {
                return BadRequest(new { error = $"CarYear must be between 1886 and {maxAllowedCarYear}." });
            }

            if (request.PriceHour <= 0m || request.PriceHour > 1_000_000m)
            {
                return BadRequest(new { error = "PriceHour must be greater than 0 and less than or equal to 1000000." });
            }

            if (request.PriceDay <= 0m || request.PriceDay > 1_000_000m)
            {
                return BadRequest(new { error = "PriceDay must be greater than 0 and less than or equal to 1000000." });
            }

            var created = await _partnerCarService.ProvisionAsync(
                new PartnerCarProvisionDto
                {
                    RelatedUserId = request.RelatedUserId,
                    CarBrand = request.CarBrand,
                    CarModel = request.CarModel,
                    CarYear = request.CarYear,
                    LicensePlate = request.LicensePlate,
                    PriceHour = request.PriceHour,
                    PriceDay = request.PriceDay,
                    OwnershipFileName = request.OwnershipDocumentFileName,
                    Images = (request.Images ?? [])
                        .Select(image => new PartnerCarProvisionImageDto
                        {
                            ImageId = image.ImageId,
                            ImageUrl = image.ImageUrl
                        })
                        .ToArray()
                },
                cancellationToken);

            return Ok(created);
        }

        private bool IsAuthorizedInternalRequest()
        {
            if (string.IsNullOrWhiteSpace(_internalAuthOptions.ApiKey))
            {
                return false;
            }

            if (!Request.Headers.TryGetValue(InternalApiKeyHeader, out var receivedApiKey))
            {
                return false;
            }

            var expectedBytes = Encoding.UTF8.GetBytes(_internalAuthOptions.ApiKey);
            var receivedBytes = Encoding.UTF8.GetBytes(receivedApiKey.ToString());

            return CryptographicOperations.FixedTimeEquals(expectedBytes, receivedBytes);
        }
    }
}
