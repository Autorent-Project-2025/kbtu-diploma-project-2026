using CarService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarService.Api.Controllers
{
    [ApiController]
    [Route("catalog")]
    public sealed class CarCatalogController : ControllerBase
    {
        private readonly ICarModelService _carModelService;

        public CarCatalogController(ICarModelService carModelService)
        {
            _carModelService = carModelService;
        }

        [HttpGet("brands")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBrands(CancellationToken cancellationToken)
        {
            var payload = await _carModelService.GetBrandsAsync(cancellationToken);
            return Ok(payload);
        }

        [HttpGet("models")]
        [AllowAnonymous]
        public async Task<IActionResult> GetModels([FromQuery] string? brand, CancellationToken cancellationToken)
        {
            var payload = await _carModelService.GetModelsAsync(brand, cancellationToken);
            return Ok(payload);
        }
    }
}
