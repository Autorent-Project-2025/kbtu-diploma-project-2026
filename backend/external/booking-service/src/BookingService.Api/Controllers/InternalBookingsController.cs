using System.Security.Cryptography;
using System.Text;
using BookingService.Application.DTOs.Booking;
using BookingService.Application.DTOs.Common;
using BookingService.Api.Options;
using BookingService.Api.Contracts.Internal;
using BookingService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;

namespace BookingService.Api.Controllers;

[ApiController]
[Route("internal/bookings")]
public sealed class InternalBookingsController : ControllerBase
{
    private const string InternalApiKeyHeader = "X-Internal-Api-Key";

    private readonly IBookingService _bookingService;
    private readonly InternalAuthOptions _internalAuthOptions;

    public InternalBookingsController(
        IBookingService bookingService,
        IOptions<InternalAuthOptions> internalAuthOptions)
    {
        _bookingService = bookingService;
        _internalAuthOptions = internalAuthOptions.Value;
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        if (!IsAuthorizedInternalRequest())
        {
            return Unauthorized(new { error = "Internal API key is invalid." });
        }

        var booking = await _bookingService.GetBookingById(id, cancellationToken);
        if (booking is null)
        {
            return NotFound(new { error = "Booking not found." });
        }

        return Ok(booking);
    }

    [AllowAnonymous]
    [HttpGet("by-partner-car/{partnerCarId:int}")]
    [HttpGet("by-car/{partnerCarId:int}")]
    public async Task<IActionResult> GetByPartnerCarId(int partnerCarId, CancellationToken cancellationToken)
    {
        if (!IsAuthorizedInternalRequest())
        {
            return Unauthorized(new { error = "Internal API key is invalid." });
        }

        var bookings = await _bookingService.GetBookingsByPartnerCarId(partnerCarId, cancellationToken);
        return Ok(bookings);
    }

    [AllowAnonymous]
    [HttpGet("by-partner-user/{partnerUserId:guid}")]
    public async Task<IActionResult> GetByPartnerUserId(Guid partnerUserId, CancellationToken cancellationToken)
    {
        if (!IsAuthorizedInternalRequest())
        {
            return Unauthorized(new { error = "Internal API key is invalid." });
        }

        var bookings = await _bookingService.GetBookingsByPartnerUserId(partnerUserId, cancellationToken);
        return Ok(bookings);
    }

    [AllowAnonymous]
    [HttpGet("counts")]
    public async Task<IActionResult> GetCounts(
        [FromQuery] string? partnerCarIds,
        [FromQuery] string? carIds,
        CancellationToken cancellationToken)
    {
        if (!IsAuthorizedInternalRequest())
        {
            return Unauthorized(new { error = "Internal API key is invalid." });
        }

        var parsedIds = ParseIds(!string.IsNullOrWhiteSpace(partnerCarIds) ? partnerCarIds : carIds);
        var counts = await _bookingService.GetBookingCountsByPartnerCarIds(parsedIds, cancellationToken);
        return Ok(counts);
    }

    [AllowAnonymous]
    [HttpPost("check-availability")]
    public async Task<IActionResult> CheckAvailability(
        [FromBody] CarAvailabilityCheckRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!IsAuthorizedInternalRequest())
        {
            return Unauthorized(new { error = "Internal API key is invalid." });
        }

        var partnerCarIds = request.CarIds
            .Where(id => id > 0)
            .Distinct()
            .ToArray();

        if (partnerCarIds.Length == 0)
        {
            return Ok(Array.Empty<CarAvailabilityResultDto>());
        }

        var payload = await _bookingService.CheckAvailabilityByPartnerCarIds(
            partnerCarIds,
            request.StartTime,
            request.EndTime,
            cancellationToken);

        return Ok(payload);
    }

    [AllowAnonymous]
    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> CancelBooking(
        int id,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] Contracts.Booking.CancelBookingRequest? request,
        CancellationToken cancellationToken)
    {
        if (!IsAuthorizedInternalRequest())
        {
            return Unauthorized(new { error = "Internal API key is invalid." });
        }

        var result = await _bookingService.CancelBookingByAdmin(id, request?.Reason, cancellationToken);
        if (!result)
        {
            return NotFound(new { error = "Booking not found or cannot be canceled." });
        }

        return Ok(new CommonResponseDto { Message = "Booking canceled." });
    }

    [AllowAnonymous]
    [HttpPost("by-user/{userId:guid}/cancel-all")]
    public async Task<IActionResult> CancelAllByUser(
        Guid userId,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] Contracts.Booking.CancelBookingRequest? request,
        CancellationToken cancellationToken)
    {
        if (!IsAuthorizedInternalRequest())
            return Unauthorized(new { error = "Internal API key is invalid." });

        var count = await _bookingService.CancelActiveBookingsByUserAsync(userId, request?.Reason, cancellationToken);
        return Ok(new { canceledCount = count });
    }

    [AllowAnonymous]
    [HttpPost("{id:int}/completion-review/approve")]
    public async Task<IActionResult> ApproveCompletionReview(
        int id,
        [FromBody] ApproveBookingCompletionReviewRequest request,
        CancellationToken cancellationToken)
    {
        if (!IsAuthorizedInternalRequest())
        {
            return Unauthorized(new { error = "Internal API key is invalid." });
        }

        await _bookingService.ProcessCompletionReviewApproved(
            id,
            request.TicketId,
            request.LatePenaltyAmount,
            request.CustomerEmail,
            request.CustomerFullName,
            cancellationToken);

        return Ok(new CommonResponseDto { Message = "Booking completion review approved." });
    }

    [AllowAnonymous]
    [HttpPost("{id:int}/completion-review/fine-issued")]
    public async Task<IActionResult> IssueCompletionReviewFine(
        int id,
        [FromBody] IssueBookingCompletionFineRequest request,
        CancellationToken cancellationToken)
    {
        if (!IsAuthorizedInternalRequest())
        {
            return Unauthorized(new { error = "Internal API key is invalid." });
        }

        await _bookingService.ProcessCompletionReviewFineIssued(
            id,
            request.TicketId,
            request.LatePenaltyAmount,
            request.DamageFineAmount,
            request.FineComment,
            request.CustomerEmail,
            request.CustomerFullName,
            cancellationToken);

        return Ok(new CommonResponseDto { Message = "Booking completion fine issued." });
    }

    [AllowAnonymous]
    [HttpPost("{id:int}/partner-cancellation/approve")]
    public async Task<IActionResult> ApprovePartnerCancellation(
        int id,
        [FromBody] ApprovePartnerBookingCancellationRequest request,
        CancellationToken cancellationToken)
    {
        if (!IsAuthorizedInternalRequest())
        {
            return Unauthorized(new { error = "Internal API key is invalid." });
        }

        await _bookingService.ProcessPartnerCancellationApproved(
            id,
            request.TicketId,
            request.PartnerReason,
            cancellationToken);

        return Ok(new CommonResponseDto { Message = "Partner cancellation request approved." });
    }

    [AllowAnonymous]
    [HttpPost("{id:int}/partner-cancellation/reject")]
    public async Task<IActionResult> RejectPartnerCancellation(
        int id,
        [FromBody] RejectPartnerBookingCancellationRequest request,
        CancellationToken cancellationToken)
    {
        if (!IsAuthorizedInternalRequest())
        {
            return Unauthorized(new { error = "Internal API key is invalid." });
        }

        await _bookingService.ProcessPartnerCancellationRejected(
            id,
            request.TicketId,
            request.DecisionReason,
            cancellationToken);

        return Ok(new CommonResponseDto { Message = "Partner cancellation request rejected." });
    }

    private static IReadOnlyCollection<int> ParseIds(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return [];
        }

        return raw
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(value => int.TryParse(value, out var parsed) ? parsed : 0)
            .Where(value => value > 0)
            .Distinct()
            .ToArray();
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
