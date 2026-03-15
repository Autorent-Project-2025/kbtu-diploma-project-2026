using ClientService.Api.Contracts.Profile;
using ClientService.Application.DTOs;
using ClientService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ClientService.Api.Controllers;

[ApiController]
[Route("profile")]
[Authorize]
public sealed class ProfileController : ControllerBase
{
    private readonly IClientService _clientService;

    public ProfileController(IClientService clientService)
    {
        _clientService = clientService;
    }

    /// <summary>
    /// Returns the Client profile record for the currently authenticated user.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
    {
        var relatedUserId = GetRelatedUserId();

        var client = await _clientService.GetByRelatedUserIdAsync(relatedUserId, cancellationToken);
        if (client is null)
        {
            return NotFound(new { error = "Client profile not found for current user." });
        }

        return Ok(client);
    }

    /// <summary>
    /// Updates firstName, lastName, birthDate, phoneNumber and avatarUrl for the current user.
    /// RelatedUserId is always taken from JWT — cannot be changed via this endpoint.
    /// </summary>
    [HttpPut]
    public async Task<IActionResult> UpdateMyProfile(
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var relatedUserId = GetRelatedUserId();

        var dto = new ProfileUpdateDto
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            BirthDate = request.BirthDate,
            PhoneNumber = request.PhoneNumber,
            AvatarUrl = request.AvatarUrl
        };

        var updated = await _clientService.UpdateByRelatedUserIdAsync(relatedUserId, dto, cancellationToken);
        if (updated is null)
        {
            return NotFound(new { error = "Client profile not found for current user." });
        }

        return Ok(updated);
    }

    private string GetRelatedUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UnauthorizedAccessException("Authenticated user id claim is required.");
        }

        return userId;
    }
}
