using IdentityService.Api.Contracts.Permissions;
using IdentityService.Application.Commands.CreatePermission;
using IdentityService.Application.Queries.GetPermissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Api.Controllers;

[ApiController]
[Authorize]
[Route("permissions")]
public sealed class PermissionsController : ControllerBase
{
    private readonly CreatePermissionCommandHandler _createPermissionCommandHandler;
    private readonly GetPermissionsQueryHandler _getPermissionsQueryHandler;

    public PermissionsController(
        CreatePermissionCommandHandler createPermissionCommandHandler,
        GetPermissionsQueryHandler getPermissionsQueryHandler)
    {
        _createPermissionCommandHandler = createPermissionCommandHandler;
        _getPermissionsQueryHandler = getPermissionsQueryHandler;
    }

    [HttpGet]
    [Authorize(Policy = "permissions:view")]
    public async Task<IActionResult> GetPermissions(CancellationToken cancellationToken)
    {
        var result = await _getPermissionsQueryHandler.Handle(new GetPermissionsQuery(), cancellationToken);
        return Ok(result.Permissions);
    }

    [HttpPost]
    [Authorize(Policy = "permissions:create")]
    public async Task<IActionResult> CreatePermission(
        [FromBody] CreatePermissionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createPermissionCommandHandler.Handle(
            new CreatePermissionCommand(request.Name, request.Description),
            cancellationToken);

        return Created($"/permissions/{result.PermissionId}", result);
    }
}
