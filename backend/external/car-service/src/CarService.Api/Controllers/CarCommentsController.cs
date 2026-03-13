using CarService.Api.Extensions;
using CarService.Application.DTOs.CarComment;
using CarService.Application.DTOs.Common;
using CarService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarService.Api.Controllers
{
    [ApiController]
    [Route("comments")]
    public sealed class CarCommentsController : ControllerBase
    {
        private readonly ICarCommentService _carCommentService;

        public CarCommentsController(ICarCommentService carCommentService)
        {
            _carCommentService = carCommentService;
        }

        [HttpGet("partner-cars/{partnerCarId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByPartnerCar(
            int partnerCarId,
            [FromQuery] PaginationParams paginationParams,
            CancellationToken cancellationToken)
        {
            var payload = await _carCommentService.GetByPartnerCarPaginatedAsync(partnerCarId, paginationParams, cancellationToken);
            return Ok(payload);
        }

        /// <summary>
        /// Returns paginated list of comments left by the currently authenticated user.
        /// Used by the profile page "My Reviews" section.
        /// </summary>
        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMyComments(
            [FromQuery] PaginationParams paginationParams,
            CancellationToken cancellationToken)
        {
            var userId = User.GetRequiredUserId();
            var payload = await _carCommentService.GetByUserIdPaginatedAsync(userId, paginationParams, cancellationToken);
            return Ok(payload);
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var payload = await _carCommentService.GetByIdAsync(id, cancellationToken);
            if (payload is null)
            {
                return NotFound(new { error = "Comment not found" });
            }

            return Ok(payload);
        }

        [HttpPost]
        [Authorize(Policy = "car-comments:create")]
        public async Task<IActionResult> Create([FromBody] CarCommentCreateDto dto, CancellationToken cancellationToken)
        {
            var userId = User.GetRequiredUserId();
            var userName = User.GetPreferredUserName();

            var created = await _carCommentService.CreateAsync(userId, userName, dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        [Authorize(Policy = "car-comments:update")]
        public async Task<IActionResult> Update(int id, [FromBody] CarCommentUpdateDto dto, CancellationToken cancellationToken)
        {
            var userId = User.GetRequiredUserId();

            var updated = await _carCommentService.UpdateAsync(userId, id, dto, cancellationToken);
            if (updated is null)
            {
                return NotFound(new { error = "Comment not found" });
            }

            return Ok(updated);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = "car-comments:delete")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var userId = User.GetRequiredUserId();
            var deleted = await _carCommentService.DeleteAsync(userId, id, cancellationToken);

            if (!deleted)
            {
                return NotFound(new { error = "Comment not found" });
            }

            return NoContent();
        }
    }
}
