using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PaymentService.Api.Contracts.Internal;
using PaymentService.Api.Options;
using PaymentService.Application.Interfaces;
using PaymentService.Application.Models;

namespace PaymentService.Api.Controllers;

[ApiController]
[Route("internal/mock-payments")]
public sealed class InternalMockPaymentsController : ControllerBase
{
    private const string InternalApiKeyHeader = "X-Internal-Api-Key";

    private readonly IMockPaymentService _mockPaymentService;
    private readonly InternalAuthOptions _internalAuthOptions;

    public InternalMockPaymentsController(
        IMockPaymentService mockPaymentService,
        IOptions<InternalAuthOptions> internalAuthOptions)
    {
        _mockPaymentService = mockPaymentService;
        _internalAuthOptions = internalAuthOptions.Value;
    }

    [HttpPost("start")]
    public async Task<IActionResult> Start(
        [FromBody] StartMockPaymentRequest request,
        CancellationToken cancellationToken)
    {
        if (!IsAuthorizedInternalRequest())
        {
            return Unauthorized(new { error = "Internal API key is invalid." });
        }

        var attempt = await _mockPaymentService.StartAsync(
            new StartMockPaymentCommand(
                request.BookingId,
                request.UserId,
                request.Amount,
                request.Currency),
            cancellationToken);

        return Ok(attempt);
    }

    [HttpGet("by-booking/{bookingId:int}")]
    public async Task<IActionResult> GetByBooking(
        int bookingId,
        [FromQuery] Guid userId,
        CancellationToken cancellationToken)
    {
        if (!IsAuthorizedInternalRequest())
        {
            return Unauthorized(new { error = "Internal API key is invalid." });
        }

        var attempt = await _mockPaymentService.GetLatestAsync(bookingId, userId, cancellationToken);
        if (attempt is null)
        {
            return NotFound(new { error = "Mock payment attempt not found." });
        }

        return Ok(attempt);
    }

    [HttpPost("{bookingId:int}/submit")]
    public async Task<IActionResult> Submit(
        int bookingId,
        [FromBody] SubmitMockPaymentRequest request,
        CancellationToken cancellationToken)
    {
        if (!IsAuthorizedInternalRequest())
        {
            return Unauthorized(new { error = "Internal API key is invalid." });
        }

        var attempt = await _mockPaymentService.SubmitAsync(
            new SubmitMockPaymentCommand(
                bookingId,
                request.UserId,
                request.SessionKey,
                request.CardHolder,
                request.CardNumber,
                request.ExpiryMonth,
                request.ExpiryYear,
                request.Cvv),
            cancellationToken);

        return Ok(attempt);
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
