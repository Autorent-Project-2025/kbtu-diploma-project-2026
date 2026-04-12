using BookingService.Application.DTOs.Common;
using BookingService.Application.DTOs.Subscription;
using BookingService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookingService.Api.Controllers
{
    [ApiController]
    [Route("subscriptions")]
    [Authorize]
    public class SubscriptionsController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;

        public SubscriptionsController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        private Guid GetUserId()
        {
            var claimUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");

            if (!Guid.TryParse(claimUserId, out var userId) || userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException("Authenticated user id claim must be a valid UUID.");
            }

            return userId;
        }

        [HttpGet("plans")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPlans(CancellationToken cancellationToken)
        {
            var result = await _subscriptionService.GetPlans(cancellationToken);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateSubscriptionDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _subscriptionService.CreateSubscription(GetUserId(), dto, cancellationToken);
            return Ok(result);
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMy(CancellationToken cancellationToken)
        {
            var result = await _subscriptionService.GetActiveSubscription(GetUserId(), cancellationToken);
            return Ok(result);
        }

        [HttpPost("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
        {
            var result = await _subscriptionService.CancelSubscription(id, GetUserId(), cancellationToken);

            if (!result)
                return NotFound(new { error = "Subscription not found" });

            return Ok(new CommonResponseDto { Message = "Subscription cancelled" });
        }
    }
}