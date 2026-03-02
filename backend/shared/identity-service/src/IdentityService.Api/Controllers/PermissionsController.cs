using IdentityService.Api.Contracts.Permissions;
using IdentityService.Application.Commands.CreatePermission;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Api.Controllers;

[ApiController]
[Authorize]
[Route("permissions")]
public sealed class PermissionsController : ControllerBase
{
    private readonly CreatePermissionCommandHandler _createPermissionCommandHandler;

    public PermissionsController(CreatePermissionCommandHandler createPermissionCommandHandler)
    {
        _createPermissionCommandHandler = createPermissionCommandHandler;
    }

    [HttpPost]
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
