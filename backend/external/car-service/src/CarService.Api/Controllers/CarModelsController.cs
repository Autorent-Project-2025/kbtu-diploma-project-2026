using CarService.Application.DTOs.CarModels;
using CarService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarService.Api.Controllers
{
    [ApiController]
    [Route("models")]
    public sealed class CarModelsController : ControllerBase
    {
        private readonly ICarModelService _carModelService;

        public CarModelsController(ICarModelService carModelService)
        {
            _carModelService = carModelService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] CarModelQueryParams queryParams, CancellationToken cancellationToken)
        {
            var payload = await _carModelService.GetAllAsync(queryParams, cancellationToken);
            return Ok(payload);
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var payload = await _carModelService.GetByIdAsync(id, cancellationToken);
            if (payload is null)
            {
                return NotFound(new { error = "Car model not found" });
            }

            return Ok(payload);
        }

        [HttpPost]
        [Authorize(Policy = "car-models:create")]
        public async Task<IActionResult> Create([FromBody] CarModelCreateDto dto, CancellationToken cancellationToken)
        {
            var created = await _carModelService.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        [Authorize(Policy = "car-models:update")]
        public async Task<IActionResult> Update(int id, [FromBody] CarModelUpdateDto dto, CancellationToken cancellationToken)
        {
            var updated = await _carModelService.UpdateAsync(id, dto, cancellationToken);
            if (updated is null)
            {
                return NotFound(new { error = "Car model not found" });
            }

            return Ok(updated);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = "car-models:delete")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var deleted = await _carModelService.DeleteAsync(id, cancellationToken);
            if (!deleted)
            {
                return NotFound(new { error = "Car model not found" });
            }

            return NoContent();
        }
    }
}
