using BookingService.Application.Constants;
using BookingService.Application.DTOs.Booking;
using BookingService.Application.DTOs.Common;
using BookingService.Application.Interfaces;
using BookingService.Application.Interfaces.Integrations;
using BookingService.Api.Contracts.Booking;
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
        private readonly IDynamicPricingService _dynamicPricingService;

        public BookingController(
            IBookingService bookingService,
            IDynamicPricingService dynamicPricingService)
        {
            _bookingService = bookingService;
            _dynamicPricingService = dynamicPricingService;
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

        [HttpGet("all")]
        [Authorize(Policy = "bookings:view")]
        public async Task<IActionResult> AllBookings([FromQuery] BookingQueryParams queryParams)
        {
            var bookings = await _bookingService.GetAllBookingsPaginated(queryParams);
            return Ok(bookings);
        }

        [HttpGet("my")]
        public async Task<IActionResult> MyBookings([FromQuery] BookingQueryParams queryParams)
        {
            var bookings = await _bookingService.GetUserBookingsPaginated(GetUserId(), queryParams);
            return Ok(bookings);
        }


        /// <summary>
        /// Returns aggregate stats for the current user's bookings:
        /// totalCount, activeCount, completedCount, totalSpent (KZT).
        /// Used by the profile page.
        /// </summary>
        [HttpGet("my/stats")]
        public async Task<IActionResult> MyStats(CancellationToken cancellationToken)
        {
            var stats = await _bookingService.GetUserBookingStats(GetUserId(), cancellationToken);
            return Ok(stats);
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

        [HttpGet("all/{id:int}")]
        [Authorize(Policy = "bookings:view")]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var booking = await _bookingService.GetBookingById(id, cancellationToken);
            if (booking == null)
            {
                return NotFound(new { error = "Booking not found" });
            }

            return Ok(booking);
        }

        [HttpPost("all/{id:int}/cancel")]
        [Authorize(Policy = "bookings:update")]
        public async Task<IActionResult> AdminCancel(int id, CancellationToken cancellationToken)
        {
            var result = await _bookingService.CancelBookingByAdmin(id, cancellationToken);
            if (!result)
            {
                return NotFound(new { error = "Booking not found" });
            }

            return Ok(new CommonResponseDto { Message = "Booking canceled" });
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

        [HttpPost("{id:int}/partner-cancel")]
        public async Task<IActionResult> PartnerCancel(int id)
        {
            var result = await _bookingService.CancelBookingByPartner(id, GetUserId());
            if (!result)
                return NotFound(new { error = "Booking not found or cannot be canceled." });
            return Ok(new CommonResponseDto { Message = "Booking canceled by partner" });
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

        [HttpPost("{id:int}/start")]
        public async Task<IActionResult> StartTrip(int id)
        {
            var result = await _bookingService.StartTrip(id, GetUserId());
            if (!result)
            {
                return NotFound(new { error = "Booking not found" });
            }

            return Ok(new CommonResponseDto { Message = "Trip started" });
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

        [HttpPost("{id:int}/complete-review")]
        public async Task<IActionResult> SubmitCompletionReview(
            int id,
            [FromForm] CompleteBookingReviewRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _bookingService.SubmitCompletionReview(
                id,
                GetUserId(),
                new BookingCompletionSubmissionDto
                {
                    CompletionFrontPhotoFile = await MapToFileUploadPayloadAsync(request.CompletionFrontPhotoFile, cancellationToken),
                    CompletionBackPhotoFile = await MapToFileUploadPayloadAsync(request.CompletionBackPhotoFile, cancellationToken),
                    CompletionSideLeftPhotoFile = await MapToFileUploadPayloadAsync(request.CompletionSideLeftPhotoFile, cancellationToken),
                    CompletionSideRightPhotoFile = await MapToFileUploadPayloadAsync(request.CompletionSideRightPhotoFile, cancellationToken),
                    CompletionInteriorPhotoFile = await MapToFileUploadPayloadAsync(request.CompletionInteriorPhotoFile, cancellationToken)
                });

            return Ok(result);
        }

        [HttpPost("{id:int}/car-comment")]
        public async Task<IActionResult> SubmitCarComment(
            int id,
            [FromBody] CreateBookingCarCommentRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _bookingService.SubmitCarComment(
                id,
                GetUserId(),
                new BookingCarCommentCreateDto
                {
                    Rating = request.Rating,
                    Content = request.Content
                },
                cancellationToken);

            return Ok(result);
        }

        [HttpGet("{id:int}/charges")]
        public async Task<IActionResult> GetCharges(int id, CancellationToken cancellationToken)
        {
            var charges = await _bookingService.GetBookingCharges(id, GetUserId(), cancellationToken);
            return Ok(charges);
        }

        [HttpPost("{id:int}/charges/{chargeId:long}/pay")]
        public async Task<IActionResult> PayCharge(int id, long chargeId, CancellationToken cancellationToken)
        {
            var charge = await _bookingService.PayBookingCharge(id, chargeId, GetUserId(), cancellationToken);
            return Ok(charge);
        }

        [HttpGet("price-preview")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPricePreview(
            [FromQuery] int partnerCarId,
            [FromQuery] DateTimeOffset startTime,
            [FromQuery] DateTimeOffset endTime,
            CancellationToken cancellationToken)
        {
            var result = await _dynamicPricingService.GetPricePreviewAsync(
                partnerCarId,
                startTime,
                endTime,
                cancellationToken);

            return Ok(result);
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

            return Ok(new { available }); // profileCommentsDTO + get-set 2 api
        }

        private static async Task<FileUploadPayload> MapToFileUploadPayloadAsync(
            IFormFile? file,
            CancellationToken cancellationToken)
        {
            if (file is null)
            {
                return new FileUploadPayload();
            }

            await using var stream = file.OpenReadStream();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream, cancellationToken);

            return new FileUploadPayload
            {
                FileName = file.FileName,
                ContentType = string.IsNullOrWhiteSpace(file.ContentType)
                    ? "application/octet-stream"
                    : file.ContentType,
                Content = memoryStream.ToArray()
            };
        }
    }
}
