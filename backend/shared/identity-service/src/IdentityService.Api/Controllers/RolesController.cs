using IdentityService.Api.Contracts.Roles;
using IdentityService.Application.Commands.AssignParentRoleToRole;
using IdentityService.Application.Commands.AssignPermissionToRole;
using IdentityService.Application.Commands.CreateRole;
using IdentityService.Application.Commands.RemoveParentRoleFromRole;
using IdentityService.Application.Commands.RemovePermissionFromRole;
using IdentityService.Application.Queries.GetRoles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Api.Controllers;

[ApiController]
[Authorize]
[Route("roles")]
public sealed class RolesController : ControllerBase
{
    private readonly CreateRoleCommandHandler _createRoleCommandHandler;
    private readonly AssignParentRoleToRoleCommandHandler _assignParentRoleToRoleCommandHandler;
    private readonly AssignPermissionToRoleCommandHandler _assignPermissionToRoleCommandHandler;
    private readonly RemoveParentRoleFromRoleCommandHandler _removeParentRoleFromRoleCommandHandler;
    private readonly RemovePermissionFromRoleCommandHandler _removePermissionFromRoleCommandHandler;
    private readonly GetRolesQueryHandler _getRolesQueryHandler;

    public RolesController(
        CreateRoleCommandHandler createRoleCommandHandler,
        AssignParentRoleToRoleCommandHandler assignParentRoleToRoleCommandHandler,
        AssignPermissionToRoleCommandHandler assignPermissionToRoleCommandHandler,
        RemoveParentRoleFromRoleCommandHandler removeParentRoleFromRoleCommandHandler,
        RemovePermissionFromRoleCommandHandler removePermissionFromRoleCommandHandler,
        GetRolesQueryHandler getRolesQueryHandler)
    {
        _createRoleCommandHandler = createRoleCommandHandler;
        _assignParentRoleToRoleCommandHandler = assignParentRoleToRoleCommandHandler;
        _assignPermissionToRoleCommandHandler = assignPermissionToRoleCommandHandler;
        _removeParentRoleFromRoleCommandHandler = removeParentRoleFromRoleCommandHandler;
        _removePermissionFromRoleCommandHandler = removePermissionFromRoleCommandHandler;
        _getRolesQueryHandler = getRolesQueryHandler;
    }

    [HttpGet]
    [Authorize(Policy = "roles:view")]
    public async Task<IActionResult> GetRoles(CancellationToken cancellationToken)
    {
        var result = await _getRolesQueryHandler.Handle(new GetRolesQuery(), cancellationToken);
        return Ok(result.Roles);
    }

    [HttpPost]
    [Authorize(Policy = "roles:create")]
    public async Task<IActionResult> CreateRole(
        [FromBody] CreateRoleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createRoleCommandHandler.Handle(
            new CreateRoleCommand(
                request.Name,
                request.PermissionIds,
                request.ParentRoleIds),
            cancellationToken);

        return Created($"/roles/{result.RoleId}", result);
    }

    [HttpPost("{id:guid}/permissions")]
    [Authorize(Policy = "roles:assign-permission")]
    public async Task<IActionResult> AssignPermission(
        [FromRoute] Guid id,
        [FromBody] AddPermissionToRoleRequest request,
        CancellationToken cancellationToken)
    {
        await _assignPermissionToRoleCommandHandler.Handle(
            new AssignPermissionToRoleCommand(id, request.PermissionId),
            cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}/permissions/{permissionId:guid}")]
    [Authorize(Policy = "roles:assign-permission")]
    public async Task<IActionResult> RemovePermission(
        [FromRoute] Guid id,
        [FromRoute] Guid permissionId,
        CancellationToken cancellationToken)
    {
        await _removePermissionFromRoleCommandHandler.Handle(
            new RemovePermissionFromRoleCommand(id, permissionId),
            cancellationToken);

        return NoContent();
    }

    [HttpPost("{id:guid}/parents")]
    [Authorize(Policy = "roles:assign-permission")]
    public async Task<IActionResult> AssignParentRole(
        [FromRoute] Guid id,
        [FromBody] AddParentRoleToRoleRequest request,
        CancellationToken cancellationToken)
    {
        await _assignParentRoleToRoleCommandHandler.Handle(
            new AssignParentRoleToRoleCommand(id, request.ParentRoleId),
            cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}/parents/{parentRoleId:guid}")]
    [Authorize(Policy = "roles:assign-permission")]
    public async Task<IActionResult> RemoveParentRole(
        [FromRoute] Guid id,
        [FromRoute] Guid parentRoleId,
        CancellationToken cancellationToken)
    {
        await _removeParentRoleFromRoleCommandHandler.Handle(
            new RemoveParentRoleFromRoleCommand(id, parentRoleId),
            cancellationToken);

        return NoContent();
    }
}
