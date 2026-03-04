using System.Security.Cryptography;
using System.Text;
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
    [HttpGet("by-car/{carId:int}")]
    public async Task<IActionResult> GetByCarId(int carId, CancellationToken cancellationToken)
    {
        if (!IsAuthorizedInternalRequest())
        {
            return Unauthorized(new { error = "Internal API key is invalid." });
        }

        var bookings = await _bookingService.GetBookingsByCarId(carId, cancellationToken);
        return Ok(bookings);
    }

    [AllowAnonymous]
    [HttpGet("counts")]
    public async Task<IActionResult> GetCounts([FromQuery] string? carIds, CancellationToken cancellationToken)
    {
        if (!IsAuthorizedInternalRequest())
        {
            return Unauthorized(new { error = "Internal API key is invalid." });
        }

        var parsedIds = ParseCarIds(carIds);
        var counts = await _bookingService.GetBookingCountsByCarIds(parsedIds, cancellationToken);
        return Ok(counts);
    }

    private static IReadOnlyCollection<int> ParseCarIds(string? raw)
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
