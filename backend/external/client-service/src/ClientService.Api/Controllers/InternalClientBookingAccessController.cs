using System.Security.Cryptography;
using System.Text;
using ClientService.Api.Contracts.Internal;
using ClientService.Api.Options;
using ClientService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ClientService.Api.Controllers;

[ApiController]
[Route("internal/clients/by-user/{relatedUserId:guid}/booking-access")]
public sealed class InternalClientBookingAccessController : ControllerBase
{
    private const string InternalApiKeyHeader = "X-Internal-Api-Key";

    private readonly IClientService _clientService;
    private readonly InternalAuthOptions _internalAuthOptions;

    public InternalClientBookingAccessController(
        IClientService clientService,
        IOptions<InternalAuthOptions> internalAuthOptions)
    {
        _clientService = clientService;
        _internalAuthOptions = internalAuthOptions.Value;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Get([FromRoute] Guid relatedUserId, CancellationToken cancellationToken)
    {
        if (!IsAuthorizedInternalRequest())
        {
            return Unauthorized(new { error = "Internal API key is invalid." });
        }

        var access = await _clientService.GetBookingAccessByRelatedUserIdAsync(
            relatedUserId.ToString(),
            cancellationToken);
        if (access is null)
        {
            return NotFound(new { error = "Client profile not found." });
        }

        return Ok(access);
    }

    [AllowAnonymous]
    [HttpPost("block")]
    public async Task<IActionResult> Block(
        [FromRoute] Guid relatedUserId,
        [FromBody] BlockBookingActionsRequest? request,
        CancellationToken cancellationToken)
    {
        if (!IsAuthorizedInternalRequest())
        {
            return Unauthorized(new { error = "Internal API key is invalid." });
        }

        var client = await _clientService.SetBookingActionsBlockedByRelatedUserIdAsync(
            relatedUserId.ToString(),
            true,
            request?.Reason,
            cancellationToken);
        if (client is null)
        {
            return NotFound(new { error = "Client profile not found." });
        }

        return Ok(client);
    }

    [AllowAnonymous]
    [HttpPost("unblock")]
    public async Task<IActionResult> Unblock([FromRoute] Guid relatedUserId, CancellationToken cancellationToken)
    {
        if (!IsAuthorizedInternalRequest())
        {
            return Unauthorized(new { error = "Internal API key is invalid." });
        }

        var client = await _clientService.SetBookingActionsBlockedByRelatedUserIdAsync(
            relatedUserId.ToString(),
            false,
            null,
            cancellationToken);
        if (client is null)
        {
            return NotFound(new { error = "Client profile not found." });
        }

        return Ok(client);
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
