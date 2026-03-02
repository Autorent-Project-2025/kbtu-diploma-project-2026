using CarService.Application.Constants;
using CarService.Application.DTOs.CarImage;
using CarService.Application.DTOs.Cars;
using CarService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarService.Api.Controllers.v2
{
    [ApiController]
    [Route("")]
    public class CarsController : ControllerBase
    {
        private readonly ICarService _carService;
        private readonly ICarFeatureService _carFeatureService;
        private readonly ICarImageService _carImageService;

        public CarsController(ICarService carService, ICarFeatureService carFeatureService, ICarImageService carImageService)
        {
            _carService = carService;
            _carFeatureService = carFeatureService;
            _carImageService = carImageService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] CarQueryParams queryParams)
        {
            try
            {
                return Ok(await _carService.GetAllWithFiltersAndSorting(queryParams));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var car = await _carService.GetById(id);
                return car == null ? NotFound() : Ok(car);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = RoleConstants.Admin)]
        public async Task<IActionResult> Create(CarCreateRequestDto dto)
        {
            try
            {
                return Ok(await _carService.Create(dto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpPost("$batch")]
        [Authorize(Roles = RoleConstants.Admin)]
        public async Task<IActionResult> Create(CarCreateRequestDto[] dtos)
        {
            try
            {
                return Ok(await _carService.Create(dtos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = RoleConstants.Admin)]
        public async Task<IActionResult> Update(int id, CarUpdateDto dto)
        {
            try
            {
                var car = await _carService.Update(id, dto);
                return car == null ? NotFound() : Ok(car);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = RoleConstants.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                return await _carService.Delete(id) ? Ok() : NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("features")]
        public async Task<IActionResult> GetAllFeatures()
        {
            try
            {
                return Ok(await _carFeatureService.GetAllAsync());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpPost("{carId}/images")]
        public async Task<IActionResult> CreateCarImage([FromRoute] int carId, CarImageCreateRequestDto dto)
        {
            try
            {
                return Ok(await _carImageService.Create(carId, dto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("{carId}/images")]
        public async Task<IActionResult> GetCarImages([FromRoute] int carId)
        {
            try
            {
                return Ok(await _carImageService.GetByCarId(carId));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}