using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PartnerService.Api.Contracts.Internal;
using PartnerService.Api.Options;
using PartnerService.Application.DTOs;
using PartnerService.Application.Interfaces;

namespace PartnerService.Api.Controllers;

[ApiController]
[Route("internal/partners")]
public sealed class InternalPartnersController : ControllerBase
{
    private const string InternalApiKeyHeader = "X-Internal-Api-Key";

    private readonly IPartnerService _partnerService;
    private readonly InternalAuthOptions _internalAuthOptions;

    public InternalPartnersController(
        IPartnerService partnerService,
        IOptions<InternalAuthOptions> internalAuthOptions)
    {
        _partnerService = partnerService;
        _internalAuthOptions = internalAuthOptions.Value;
    }

    [AllowAnonymous]
    [HttpPost("provision")]
    public async Task<IActionResult> Provision(
        [FromBody] ProvisionPartnerRequest request,
        CancellationToken cancellationToken)
    {
        if (!IsAuthorizedInternalRequest())
        {
            return Unauthorized(new { error = "Internal API key is invalid." });
        }

        if (request.RelatedUserId == Guid.Empty)
        {
            return BadRequest(new { error = "RelatedUserId is required." });
        }

        var created = await _partnerService.CreateAsync(
            new PartnerCreateDto
            {
                OwnerFirstName = request.OwnerFirstName,
                OwnerLastName = request.OwnerLastName,
                ContractFileName = request.ContractFileName,
                OwnerIdentityFileName = request.OwnerIdentityFileName,
                RegistrationDate = request.RegistrationDate,
                PartnershipEndDate = request.PartnershipEndDate,
                RelatedUserId = request.RelatedUserId.ToString(),
                PhoneNumber = request.PhoneNumber,
                ProvisionRequestKey = request.ProvisionRequestKey
            },
            cancellationToken);

        return Ok(created);
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
