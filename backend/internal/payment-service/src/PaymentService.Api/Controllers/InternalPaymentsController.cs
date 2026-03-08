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
[Route("internal/payments")]
public sealed class InternalPaymentsController : ControllerBase
{
    private const string InternalApiKeyHeader = "X-Internal-Api-Key";

    private readonly IPaymentLedgerService _paymentLedgerService;
    private readonly InternalAuthOptions _internalAuthOptions;

    public InternalPaymentsController(
        IPaymentLedgerService paymentLedgerService,
        IOptions<InternalAuthOptions> internalAuthOptions)
    {
        _paymentLedgerService = paymentLedgerService;
        _internalAuthOptions = internalAuthOptions.Value;
    }

    [HttpPost("bookings/confirm")]
    public async Task<IActionResult> ConfirmBooking(
        [FromBody] ConfirmBookingPaymentRequest request,
        CancellationToken cancellationToken)
    {
        if (!IsAuthorizedInternalRequest())
        {
            return Unauthorized(new { error = "Internal API key is invalid." });
        }

        await _paymentLedgerService.RecordBookingConfirmedAsync(
            new BookingPaymentSnapshot(
                request.BookingId,
                request.UserId,
                request.PartnerUserId,
                request.PartnerCarId,
                request.PriceHour,
                request.TotalPrice),
            cancellationToken);

        return Ok(new { message = "Booking payment confirmed." });
    }

    [HttpPost("bookings/cancel")]
    public async Task<IActionResult> CancelBooking(
        [FromBody] BookingPaymentActionRequest request,
        CancellationToken cancellationToken)
    {
        if (!IsAuthorizedInternalRequest())
        {
            return Unauthorized(new { error = "Internal API key is invalid." });
        }

        await _paymentLedgerService.RecordBookingCanceledAsync(request.BookingId, cancellationToken);
        return Ok(new { message = "Booking payment canceled." });
    }

    [HttpPost("bookings/complete")]
    public async Task<IActionResult> CompleteBooking(
        [FromBody] BookingPaymentActionRequest request,
        CancellationToken cancellationToken)
    {
        if (!IsAuthorizedInternalRequest())
        {
            return Unauthorized(new { error = "Internal API key is invalid." });
        }

        await _paymentLedgerService.RecordBookingCompletedAsync(request.BookingId, cancellationToken);
        return Ok(new { message = "Booking payment completed." });
    }

    [HttpPost("payouts/request")]
    public async Task<IActionResult> RequestPayout(
        [FromBody] RequestPartnerPayoutRequest request,
        CancellationToken cancellationToken)
    {
        if (!IsAuthorizedInternalRequest())
        {
            return Unauthorized(new { error = "Internal API key is invalid." });
        }

        var payout = await _paymentLedgerService.RequestPayoutAsync(
            request.PartnerUserId,
            request.Amount,
            request.RequestKey,
            cancellationToken);

        return Ok(payout);
    }

    [HttpPost("payouts/{payoutId:long}/processing")]
    public async Task<IActionResult> MarkPayoutProcessing(long payoutId, CancellationToken cancellationToken)
    {
        if (!IsAuthorizedInternalRequest())
        {
            return Unauthorized(new { error = "Internal API key is invalid." });
        }

        var payout = await _paymentLedgerService.MarkPayoutProcessingAsync(payoutId, cancellationToken);
        return Ok(payout);
    }

    [HttpPost("payouts/{payoutId:long}/paid")]
    public async Task<IActionResult> MarkPayoutPaid(long payoutId, CancellationToken cancellationToken)
    {
        if (!IsAuthorizedInternalRequest())
        {
            return Unauthorized(new { error = "Internal API key is invalid." });
        }

        var payout = await _paymentLedgerService.MarkPayoutPaidAsync(payoutId, cancellationToken);
        return Ok(payout);
    }

    [HttpPost("payouts/{payoutId:long}/failed")]
    public async Task<IActionResult> MarkPayoutFailed(
        long payoutId,
        [FromBody] PayoutFailureRequest request,
        CancellationToken cancellationToken)
    {
        if (!IsAuthorizedInternalRequest())
        {
            return Unauthorized(new { error = "Internal API key is invalid." });
        }

        var payout = await _paymentLedgerService.MarkPayoutFailedAsync(
            payoutId,
            request.Reason,
            cancellationToken);

        return Ok(payout);
    }

    [HttpPost("payouts/{payoutId:long}/cancel")]
    public async Task<IActionResult> CancelPayout(
        long payoutId,
        [FromBody] PayoutFailureRequest request,
        CancellationToken cancellationToken)
    {
        if (!IsAuthorizedInternalRequest())
        {
            return Unauthorized(new { error = "Internal API key is invalid." });
        }

        var payout = await _paymentLedgerService.CancelPayoutAsync(
            payoutId,
            request.Reason,
            cancellationToken);

        return Ok(payout);
    }

    [HttpGet("payouts/{payoutId:long}")]
    public async Task<IActionResult> GetPayout(long payoutId, CancellationToken cancellationToken)
    {
        if (!IsAuthorizedInternalRequest())
        {
            return Unauthorized(new { error = "Internal API key is invalid." });
        }

        var payout = await _paymentLedgerService.GetPayoutAsync(payoutId, cancellationToken);
        if (payout is null)
        {
            return NotFound(new { error = "Partner payout not found." });
        }

        return Ok(payout);
    }

    [HttpGet("payouts/by-partner/{partnerUserId:guid}")]
    public async Task<IActionResult> GetPayouts(
        Guid partnerUserId,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
    {
        if (!IsAuthorizedInternalRequest())
        {
            return Unauthorized(new { error = "Internal API key is invalid." });
        }

        var payouts = await _paymentLedgerService.GetPayoutsAsync(partnerUserId, take, cancellationToken);
        return Ok(payouts);
    }

    [HttpGet("wallets/{partnerUserId:guid}")]
    public async Task<IActionResult> GetWallet(Guid partnerUserId, CancellationToken cancellationToken)
    {
        if (!IsAuthorizedInternalRequest())
        {
            return Unauthorized(new { error = "Internal API key is invalid." });
        }

        var wallet = await _paymentLedgerService.GetWalletAsync(partnerUserId, cancellationToken);
        if (wallet is null)
        {
            return NotFound(new { error = "Partner wallet not found." });
        }

        return Ok(wallet);
    }

    [HttpGet("ledger/{partnerUserId:guid}")]
    public async Task<IActionResult> GetLedger(
        Guid partnerUserId,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
    {
        if (!IsAuthorizedInternalRequest())
        {
            return Unauthorized(new { error = "Internal API key is invalid." });
        }

        var ledger = await _paymentLedgerService.GetLedgerAsync(partnerUserId, take, cancellationToken);
        return Ok(ledger);
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
