using BookingService.Application.Constants;
using BookingService.Application.DTOs.Booking;
using BookingService.Application.DTOs.Common;
using BookingService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookingService.Api.Controllers
{
    [ApiController]
    [Route("")]
    [Authorize]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        private string GetUserId()
        {
            var claimUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");

            if (!string.IsNullOrWhiteSpace(claimUserId))
            {
                return claimUserId;
            }

            throw new UnauthorizedAccessException("Authenticated user id claim is required.");
        }

        [HttpPost]
        [Authorize(Policy = "bookings:create")]
        public async Task<IActionResult> Create([FromBody] BookingCreateDto dto)
        {
            var booking = await _bookingService.CreateBooking(GetUserId(), dto);
            return CreatedAtAction(nameof(Get), new { id = booking.Id }, booking);
        }

        [HttpGet("my")]
        public async Task<IActionResult> MyBookings([FromQuery] BookingQueryParams queryParams)
        {
            var bookings = await _bookingService.GetUserBookingsPaginated(GetUserId(), queryParams);
            return Ok(bookings);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var booking = await _bookingService.GetBooking(id, GetUserId());
            if (booking == null)
            {
                return NotFound(new { error = "Booking not found" });
            }

            return Ok(booking);
        }

        [HttpPost("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var result = await _bookingService.CancelBooking(id, GetUserId());
            if (!result)
            {
                return NotFound(new { error = "Booking not found" });
            }

            return Ok(new CommonResponseDto { Message = "Booking canceled" });
        }

        [HttpPost("{id:int}/confirm")]
        public async Task<IActionResult> Confirm(int id)
        {
            var result = await _bookingService.ConfirmBooking(id, GetUserId());
            if (!result)
            {
                return NotFound(new { error = "Booking not found" });
            }

            return Ok(new CommonResponseDto { Message = "Booking confirmed" });
        }

        [HttpPost("{id:int}/complete")]
        public async Task<IActionResult> Complete(int id)
        {
            var result = await _bookingService.CompleteBooking(id, GetUserId());
            if (!result)
            {
                return NotFound(new { error = "Booking not found" });
            }

            return Ok(new CommonResponseDto { Message = "Booking completed" });
        }

        [HttpGet("available")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckAvailable([FromQuery] int carId, [FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            var available = await _bookingService.IsCarAvailable(carId, start, end);
            return Ok(new { available });
        }
    }
}
