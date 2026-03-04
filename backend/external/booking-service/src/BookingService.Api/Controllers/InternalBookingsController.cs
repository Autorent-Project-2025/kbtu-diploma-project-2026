using System.Security.Cryptography;
using System.Text;
using BookingService.Application.DTOs.Booking;
using BookingService.Api.Options;
using BookingService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
