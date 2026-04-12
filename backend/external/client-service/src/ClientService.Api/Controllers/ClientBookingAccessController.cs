using ClientService.Api.Contracts.Internal;
using ClientService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClientService.Api.Controllers;

[ApiController]
[Route("clients/by-user/{relatedUserId:guid}/booking-access")]
[Authorize]
public sealed class ClientBookingAccessController : ControllerBase
{
    private readonly IClientService _clientService;

    public ClientBookingAccessController(IClientService clientService)
        => _clientService = clientService;

    [HttpGet]
    [Authorize(Policy = "clients:block")]
    public async Task<IActionResult> Get(
        [FromRoute] Guid relatedUserId,
        CancellationToken cancellationToken)
    {
        var access = await _clientService.GetBookingAccessByRelatedUserIdAsync(
            relatedUserId.ToString(), cancellationToken);

        if (access is null)
            return NotFound(new { error = "Client profile not found." });

        return Ok(access);
    }

    [HttpPost("block")]
    [Authorize(Policy = "clients:block")]
    public async Task<IActionResult> Block(
        [FromRoute] Guid relatedUserId,
        [FromBody] BlockBookingActionsRequest? request,
        CancellationToken cancellationToken)
    {
        var client = await _clientService.SetBookingActionsBlockedByRelatedUserIdAsync(
            relatedUserId.ToString(), true, request?.Reason, cancellationToken);

        if (client is null)
            return NotFound(new { error = "Client profile not found." });

        return Ok(client);
    }

    [HttpPost("unblock")]
    [Authorize(Policy = "clients:block")]
    public async Task<IActionResult> Unblock(
        [FromRoute] Guid relatedUserId,
        CancellationToken cancellationToken)
    {
        var client = await _clientService.SetBookingActionsBlockedByRelatedUserIdAsync(
            relatedUserId.ToString(), false, null, cancellationToken);

        if (client is null)
            return NotFound(new { error = "Client profile not found." });

        return Ok(client);
    }
}
