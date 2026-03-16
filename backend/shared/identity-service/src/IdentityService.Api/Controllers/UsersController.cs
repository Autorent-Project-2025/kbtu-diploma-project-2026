using IdentityService.Api.Contracts.Users;
using IdentityService.Application.Commands.ActivateUserByAdmin;
using IdentityService.Application.Commands.AssignRoleToUser;
using IdentityService.Application.Commands.CreateUser;
using IdentityService.Application.Commands.DeactivateUser;
using IdentityService.Application.Commands.DeleteUser;
using IdentityService.Application.Commands.RemoveRoleFromUser;
using IdentityService.Application.Commands.UpdateUser;
using IdentityService.Application.Exceptions;
using IdentityService.Application.Queries.GetUserById;
using IdentityService.Application.Queries.GetUsers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IdentityService.Api.Controllers;

[ApiController]
[Authorize]
[Route("users")]
public sealed class UsersController : ControllerBase
{
    private readonly ActivateUserByAdminCommandHandler _activateUserByAdminCommandHandler;
    private readonly AssignRoleToUserCommandHandler _assignRoleToUserCommandHandler;
    private readonly CreateUserCommandHandler _createUserCommandHandler;
    private readonly DeactivateUserCommandHandler _deactivateUserCommandHandler;
    private readonly DeleteUserCommandHandler _deleteUserCommandHandler;
    private readonly GetUserByIdQueryHandler _getUserByIdQueryHandler;
    private readonly GetUsersQueryHandler _getUsersQueryHandler;
    private readonly RemoveRoleFromUserCommandHandler _removeRoleFromUserCommandHandler;
    private readonly UpdateUserCommandHandler _updateUserCommandHandler;

    public UsersController(
        ActivateUserByAdminCommandHandler activateUserByAdminCommandHandler,
        AssignRoleToUserCommandHandler assignRoleToUserCommandHandler,
        CreateUserCommandHandler createUserCommandHandler,
        DeactivateUserCommandHandler deactivateUserCommandHandler,
        DeleteUserCommandHandler deleteUserCommandHandler,
        GetUserByIdQueryHandler getUserByIdQueryHandler,
        GetUsersQueryHandler getUsersQueryHandler,
        RemoveRoleFromUserCommandHandler removeRoleFromUserCommandHandler,
        UpdateUserCommandHandler updateUserCommandHandler)
    {
        _activateUserByAdminCommandHandler = activateUserByAdminCommandHandler;
        _assignRoleToUserCommandHandler = assignRoleToUserCommandHandler;
        _createUserCommandHandler = createUserCommandHandler;
        _deactivateUserCommandHandler = deactivateUserCommandHandler;
        _deleteUserCommandHandler = deleteUserCommandHandler;
        _getUserByIdQueryHandler = getUserByIdQueryHandler;
        _getUsersQueryHandler = getUsersQueryHandler;
        _removeRoleFromUserCommandHandler = removeRoleFromUserCommandHandler;
        _updateUserCommandHandler = updateUserCommandHandler;
    }

    [HttpGet]
    [Authorize(Policy = "users:view")]
    public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
    {
        var result = await _getUsersQueryHandler.Handle(new GetUsersQuery(), cancellationToken);
        return Ok(result.Users);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "users:view")]
    public async Task<IActionResult> GetUserById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _getUserByIdQueryHandler.Handle(new GetUserByIdQuery(id), cancellationToken);
        return Ok(result.User);
    }

    [HttpPost]
    [Authorize(Policy = "users:create")]
    public async Task<IActionResult> CreateUser(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createUserCommandHandler.Handle(
            new CreateUserCommand(
                request.Username,
                request.Email,
                request.Password,
                request.Roles,
                request.SubjectType,
                request.ActorType),
            cancellationToken);

        return Created($"/users/{result.UserId}", result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "users:update")]
    public async Task<IActionResult> UpdateUser(
        [FromRoute] Guid id,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _updateUserCommandHandler.Handle(
            new UpdateUserCommand(
                id,
                request.Username,
                request.Email,
                request.SubjectType,
                request.ActorType),
            cancellationToken);

        return Ok(result.User);
    }

    [HttpPost("{id:guid}/roles")]
    [Authorize(Policy = "users:assign-role")]
    public async Task<IActionResult> AssignRoleToUser(
        [FromRoute] Guid id,
        [FromBody] AssignRoleToUserRequest request,
        CancellationToken cancellationToken)
    {
        await _assignRoleToUserCommandHandler.Handle(
            new AssignRoleToUserCommand(id, request.RoleId),
            cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}/roles/{roleId:guid}")]
    [Authorize(Policy = "users:remove-role")]
    public async Task<IActionResult> RemoveRoleFromUser(
        [FromRoute] Guid id,
        [FromRoute] Guid roleId,
        CancellationToken cancellationToken)
    {
        await _removeRoleFromUserCommandHandler.Handle(
            new RemoveRoleFromUserCommand(id, roleId),
            cancellationToken);

        return NoContent();
    }

    [HttpPatch("{id:guid}/activate")]
    [Authorize(Policy = "users:activate")]
    public async Task<IActionResult> ActivateUser(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await _activateUserByAdminCommandHandler.Handle(
            new ActivateUserByAdminCommand(id),
            cancellationToken);

        return NoContent();
    }

    [HttpPatch("{id:guid}/deactivate")]
    [Authorize(Policy = "users:deactivate")]
    public async Task<IActionResult> DeactivateUser(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        EnsureNotCurrentUser(id);

        await _deactivateUserCommandHandler.Handle(
            new DeactivateUserCommand(id),
            cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "users:delete")]
    public async Task<IActionResult> DeleteUser(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        EnsureNotCurrentUser(id);

        await _deleteUserCommandHandler.Handle(new DeleteUserCommand(id), cancellationToken);
        return NoContent();
    }

    private void EnsureNotCurrentUser(Guid userId)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (!Guid.TryParse(currentUserId, out var actorId))
        {
            return;
        }

        if (actorId == userId)
        {
            throw new ValidationException("You cannot perform this operation on your own account.");
        }
    }
}
