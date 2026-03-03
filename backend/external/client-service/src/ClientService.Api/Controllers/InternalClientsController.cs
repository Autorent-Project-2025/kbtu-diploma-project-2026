using System.Security.Cryptography;
using System.Text;
using ClientService.Api.Contracts.Internal;
using ClientService.Api.Options;
using ClientService.Application.DTOs;
using ClientService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ClientService.Api.Controllers;

[ApiController]
[Route("internal/clients")]
public sealed class InternalClientsController : ControllerBase
{
    private const string InternalApiKeyHeader = "X-Internal-Api-Key";

    private readonly IClientService _clientService;
    private readonly InternalAuthOptions _internalAuthOptions;

    public InternalClientsController(
        IClientService clientService,
        IOptions<InternalAuthOptions> internalAuthOptions)
    {
        _clientService = clientService;
        _internalAuthOptions = internalAuthOptions.Value;
    }

    [AllowAnonymous]
    [HttpPost("provision")]
    public async Task<IActionResult> Provision(
        [FromBody] ProvisionClientRequest request,
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

        var created = await _clientService.CreateAsync(
            new ClientCreateDto
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                BirthDate = request.BirthDate,
                IdentityDocumentFileName = request.IdentityDocumentFileName,
                DriverLicenseFileName = request.DriverLicenseFileName,
                RelatedUserId = request.RelatedUserId.ToString(),
                PhoneNumber = request.PhoneNumber,
                AvatarUrl = request.AvatarUrl
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
