using PartnerService.Api.Contracts.Partners;
using PartnerService.Application.DTOs;
using PartnerService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace PartnerService.Api.Controllers;

[ApiController]
[Route("")]
[Authorize]
public sealed class PartnersController : ControllerBase
{
    private readonly IPartnerService _partnerService;
    private readonly IFileStorageClient _fileStorageClient;
    private readonly IPartnerPaymentClient _partnerPaymentClient;
    private readonly IPartnerBookingClient _partnerBookingClient;

    public PartnersController(
        IPartnerService partnerService,
        IFileStorageClient fileStorageClient,
        IPartnerPaymentClient partnerPaymentClient,
        IPartnerBookingClient partnerBookingClient)
    {
        _partnerService = partnerService;
        _fileStorageClient = fileStorageClient;
        _partnerPaymentClient = partnerPaymentClient;
        _partnerBookingClient = partnerBookingClient;
    }

    [HttpGet]
    [Authorize(Policy = "partners:view")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var partners = await _partnerService.GetAllAsync(cancellationToken);
        return Ok(partners);
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "partners:view")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var partner = await _partnerService.GetByIdAsync(id, cancellationToken);
        if (partner is null)
        {
            return NotFound(new { error = "Partner not found" });
        }

        return Ok(partner);
    }

    [HttpPost]
    [Authorize(Policy = "partners:create")]
    public async Task<IActionResult> Create([FromBody] CreatePartnerRequest request, CancellationToken cancellationToken)
    {
        var created = await _partnerService.CreateAsync(MapToCreateDto(request), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "partners:update")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePartnerRequest request, CancellationToken cancellationToken)
    {
        var updated = await _partnerService.UpdateAsync(id, MapToUpdateDto(request), cancellationToken);
        if (updated is null)
        {
            return NotFound(new { error = "Partner not found" });
        }

        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "partners:delete")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _partnerService.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound(new { error = "Partner not found" });
        }

        return NoContent();
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var userId = ResolveCurrentUserId();
        var partner = await _partnerService.GetByRelatedUserIdAsync(userId, cancellationToken);
        if (partner is null)
        {
            return NotFound(new { error = "Partner not found for current user" });
        }

        return Ok(partner);
    }

    [HttpGet("me/files/temporary-link")]
    public async Task<IActionResult> GetMyFileTemporaryLink([FromQuery] string? fileName, CancellationToken cancellationToken)
    {
        var userId = ResolveCurrentUserId();
        var partner = await _partnerService.GetByRelatedUserIdAsync(userId, cancellationToken);
        if (partner is null)
        {
            return NotFound(new { error = "Partner not found for current user" });
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            return BadRequest(new { error = "fileName is required." });
        }

        var payload = await _fileStorageClient.GetTemporaryLinkAsync(fileName, cancellationToken: cancellationToken);
        return Ok(payload);
    }

    [HttpGet("me/wallet")]
    public async Task<IActionResult> GetMyWallet(CancellationToken cancellationToken)
    {
        var partnerUserId = await ResolveCurrentPartnerUserIdAsync(cancellationToken);
        var wallet = await _partnerPaymentClient.GetWalletAsync(partnerUserId, cancellationToken);
        return Ok(wallet);
    }

    [HttpGet("me/ledger")]
    public async Task<IActionResult> GetMyLedger([FromQuery] int take = 50, CancellationToken cancellationToken = default)
    {
        var partnerUserId = await ResolveCurrentPartnerUserIdAsync(cancellationToken);
        var ledger = await _partnerPaymentClient.GetLedgerAsync(partnerUserId, take, cancellationToken);
        return Ok(ledger);
    }

    [HttpGet("me/payouts")]
    public async Task<IActionResult> GetMyPayouts([FromQuery] int take = 50, CancellationToken cancellationToken = default)
    {
        var partnerUserId = await ResolveCurrentPartnerUserIdAsync(cancellationToken);
        var payouts = await _partnerPaymentClient.GetPayoutsAsync(partnerUserId, take, cancellationToken);
        return Ok(payouts);
    }

    [HttpGet("me/bookings")]
    public async Task<IActionResult> GetMyBookings(CancellationToken cancellationToken)
    {
        var partnerUserId = await ResolveCurrentPartnerUserIdAsync(cancellationToken);
        var bookings = await _partnerBookingClient.GetBookingsAsync(partnerUserId, cancellationToken);
        return Ok(bookings);
    }

    [HttpGet("me/payouts/{payoutId:long}")]
    public async Task<IActionResult> GetMyPayout(long payoutId, CancellationToken cancellationToken)
    {
        var partnerUserId = await ResolveCurrentPartnerUserIdAsync(cancellationToken);
        var payout = await _partnerPaymentClient.GetPayoutAsync(payoutId, cancellationToken);
        if (payout is null || payout.PartnerUserId != partnerUserId)
        {
            return NotFound(new { error = "Payout not found." });
        }

        return Ok(payout);
    }

    [HttpPost("me/payouts")]
    public async Task<IActionResult> RequestMyPayout(
        [FromBody] RequestPartnerPayoutRequest request,
        CancellationToken cancellationToken)
    {
        var partnerUserId = await ResolveCurrentPartnerUserIdAsync(cancellationToken);
        var payout = await _partnerPaymentClient.RequestPayoutAsync(
            partnerUserId,
            request.Amount,
            request.RequestKey,
            cancellationToken);

        return Ok(payout);
    }

    [HttpPost("me/payouts/{payoutId:long}/cancel")]
    public async Task<IActionResult> CancelMyPayout(
        long payoutId,
        [FromBody] CancelPartnerPayoutRequest request,
        CancellationToken cancellationToken)
    {
        var partnerUserId = await ResolveCurrentPartnerUserIdAsync(cancellationToken);
        var existingPayout = await _partnerPaymentClient.GetPayoutAsync(payoutId, cancellationToken);
        if (existingPayout is null || existingPayout.PartnerUserId != partnerUserId)
        {
            return NotFound(new { error = "Payout not found." });
        }

        var payout = await _partnerPaymentClient.CancelPayoutAsync(payoutId, request.Reason, cancellationToken);
        return Ok(payout);
    }

    [AllowAnonymous]
    [HttpGet("public/by-related-user/{relatedUserId}")]
    public async Task<IActionResult> GetPublicByRelatedUserId(string relatedUserId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(relatedUserId))
        {
            return BadRequest(new { error = "Related user id is required." });
        }

        var partner = await _partnerService.GetByRelatedUserIdAsync(relatedUserId, cancellationToken);
        if (partner is null)
        {
            return NotFound(new { error = "Partner not found." });
        }

        return Ok(new PublicPartnerProfileResponse
        {
            RelatedUserId = partner.RelatedUserId,
            CarrierName = $"{partner.OwnerFirstName} {partner.OwnerLastName}".Trim()
        });
    }

    private static PartnerCreateDto MapToCreateDto(CreatePartnerRequest request)
    {
        return new PartnerCreateDto
        {
            OwnerFirstName = request.OwnerFirstName,
            OwnerLastName = request.OwnerLastName,
            ContractFileName = request.ContractFileName,
            OwnerIdentityFileName = request.OwnerIdentityFileName,
            RegistrationDate = request.RegistrationDate,
            PartnershipEndDate = request.PartnershipEndDate,
            RelatedUserId = request.RelatedUserId,
            PhoneNumber = request.PhoneNumber
        };
    }

    private static PartnerUpdateDto MapToUpdateDto(UpdatePartnerRequest request)
    {
        return new PartnerUpdateDto
        {
            OwnerFirstName = request.OwnerFirstName,
            OwnerLastName = request.OwnerLastName,
            ContractFileName = request.ContractFileName,
            OwnerIdentityFileName = request.OwnerIdentityFileName,
            RegistrationDate = request.RegistrationDate,
            PartnershipEndDate = request.PartnershipEndDate,
            RelatedUserId = request.RelatedUserId,
            PhoneNumber = request.PhoneNumber
        };
    }

    private string ResolveCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UnauthorizedAccessException("Authenticated user id claim is required.");
        }

        return userId;
    }

    private async Task<Guid> ResolveCurrentPartnerUserIdAsync(CancellationToken cancellationToken)
    {
        var userId = ResolveCurrentUserId();
        var partner = await _partnerService.GetByRelatedUserIdAsync(userId, cancellationToken);
        if (partner is null)
        {
            throw new KeyNotFoundException("Partner not found for current user.");
        }

        if (!Guid.TryParse(userId, out var partnerUserId) || partnerUserId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Authenticated user id must be a valid GUID.");
        }

        return partnerUserId;
    }
}
