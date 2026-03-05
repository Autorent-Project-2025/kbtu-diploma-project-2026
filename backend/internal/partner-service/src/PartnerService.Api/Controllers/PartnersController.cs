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

    public PartnersController(IPartnerService partnerService)
    {
        _partnerService = partnerService;
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
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UnauthorizedAccessException("Authenticated user id claim is required.");
        }

        var partner = await _partnerService.GetByRelatedUserIdAsync(userId, cancellationToken);
        if (partner is null)
        {
            return NotFound(new { error = "Partner not found for current user" });
        }

        return Ok(partner);
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
}
