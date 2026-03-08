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

        [HttpPost("{id:int}/payment/start")]
        public async Task<IActionResult> StartPayment(int id)
        {
            var payment = await _bookingService.StartPayment(id, GetUserId());
            return Ok(payment);
        }

        [HttpGet("{id:int}/payment/status")]
        public async Task<IActionResult> GetPaymentStatus(int id)
        {
            var payment = await _bookingService.GetPaymentStatus(id, GetUserId());
            return Ok(payment);
        }

        [HttpPost("{id:int}/payment/submit")]
        public async Task<IActionResult> SubmitPayment(int id, [FromBody] BookingPaymentSubmitRequestDto dto)
        {
            var payment = await _bookingService.SubmitPayment(id, GetUserId(), dto);
            return Ok(payment);
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
        public async Task<IActionResult> CheckAvailable(
            [FromQuery] int partnerCarId,
            [FromQuery] DateTimeOffset startTime,
            [FromQuery] DateTimeOffset endTime)
        {
            if (partnerCarId <= 0)
            {
                throw new ArgumentException("partnerCarId is required and must be greater than zero.");
            }

            var available = await _bookingService.IsPartnerCarAvailable(
                partnerCarId,
                startTime,
                endTime);

            return Ok(new { available });
        }
    }
}
