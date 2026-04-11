using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Application.Interfaces;

namespace PaymentService.Api.Controllers;

[ApiController]
[Route("view")]
public sealed class PaymentsViewController : ControllerBase
{
    private readonly IPaymentLedgerService _paymentLedgerService;

    public PaymentsViewController(IPaymentLedgerService paymentLedgerService)
    {
        _paymentLedgerService = paymentLedgerService;
    }

    [Authorize(Policy = "payments:view")]
    [HttpGet("bookings/{bookingId:int}/charges")]
    public async Task<IActionResult> GetBookingCharges(int bookingId, CancellationToken cancellationToken)
    {
        var charges = await _paymentLedgerService.GetBookingChargesAsync(bookingId, cancellationToken);
        return Ok(charges);
    }
}
