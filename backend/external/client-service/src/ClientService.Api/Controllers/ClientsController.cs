using ClientService.Api.Contracts.Clients;
using ClientService.Application.DTOs;
using ClientService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ClientService.Api.Controllers;

[ApiController]
[Route("")]
[Authorize]
public class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;

    public ClientsController(IClientService clientService)
    {
        _clientService = clientService;
    }

    [HttpGet]
    [Authorize(Policy = "clients:view")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var clients = await _clientService.GetAllAsync(cancellationToken);
        return Ok(clients);
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "clients:view")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var client = await _clientService.GetByIdAsync(id, cancellationToken);
        if (client is null)
        {
            return NotFound(new { error = "Client not found" });
        }

        return Ok(client);
    }

    [HttpPost]
    [Authorize(Policy = "clients:create")]
    public async Task<IActionResult> Create([FromBody] CreateClientRequest request, CancellationToken cancellationToken)
    {
        var created = await _clientService.CreateAsync(MapToCreateDto(request), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "clients:update")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateClientRequest request, CancellationToken cancellationToken)
    {
        var updated = await _clientService.UpdateAsync(id, MapToUpdateDto(request), cancellationToken);
        if (updated is null)
        {
            return NotFound(new { error = "Client not found" });
        }

        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "clients:delete")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _clientService.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound(new { error = "Client not found" });
        }

        return NoContent();
    }

    [HttpGet("me")]
    public IActionResult Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UnauthorizedAccessException("Authenticated user id claim is required.");
        }

        var username = User.FindFirstValue("username") ?? User.FindFirstValue(ClaimTypes.Name);
        var permissions = User.FindAll("permissions")
            .Select(claim => claim.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var roles = User.FindAll(ClaimTypes.Role)
            .Select(claim => claim.Value)
            .Concat(User.FindAll("role").Select(claim => claim.Value))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var response = new MeResponseDto
        {
            UserId = userId,
            Username = username,
            Permissions = permissions,
            Roles = roles,
            IssuedAtUtc = ParseTimestamp(
                User.FindFirstValue(JwtRegisteredClaimNames.Iat) ?? User.FindFirstValue("iat")),
            ExpiresAtUtc = ParseTimestamp(
                User.FindFirstValue(JwtRegisteredClaimNames.Exp) ?? User.FindFirstValue("exp"))
        };

        return Ok(response);
    }

    private static ClientCreateDto MapToCreateDto(CreateClientRequest request)
    {
        return new ClientCreateDto
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            BirthDate = request.BirthDate,
            IdentityDocumentFileName = request.IdentityDocumentFileName,
            DriverLicenseFileName = request.DriverLicenseFileName,
            RelatedUserId = request.RelatedUserId,
            PhoneNumber = request.PhoneNumber,
            AvatarUrl = request.AvatarUrl
        };
    }

    private static ClientUpdateDto MapToUpdateDto(UpdateClientRequest request)
    {
        return new ClientUpdateDto
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            BirthDate = request.BirthDate,
            IdentityDocumentFileName = request.IdentityDocumentFileName,
            DriverLicenseFileName = request.DriverLicenseFileName,
            RelatedUserId = request.RelatedUserId,
            PhoneNumber = request.PhoneNumber,
            AvatarUrl = request.AvatarUrl
        };
    }

    private static DateTimeOffset? ParseTimestamp(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (long.TryParse(value, out var unixSeconds))
        {
            try
            {
                return DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }
        }

        if (DateTimeOffset.TryParse(value, out var parsedDateTimeOffset))
        {
            return parsedDateTimeOffset.ToUniversalTime();
        }

        return null;
    }
}
