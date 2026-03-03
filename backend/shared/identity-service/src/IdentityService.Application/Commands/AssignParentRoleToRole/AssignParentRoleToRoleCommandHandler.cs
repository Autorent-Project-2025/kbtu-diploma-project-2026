using IdentityService.Application.Exceptions;
using IdentityService.Application.Interfaces;
using IdentityService.Application.Utils;

namespace IdentityService.Application.Commands.AssignParentRoleToRole;

public sealed class AssignParentRoleToRoleCommandHandler
{
    private readonly IRoleRepository _roleRepository;
    private readonly IIdentityUnitOfWork _unitOfWork;

    public AssignParentRoleToRoleCommandHandler(
        IRoleRepository roleRepository,
        IIdentityUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        AssignParentRoleToRoleCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.RoleId == command.ParentRoleId)
        {
            throw new ValidationException("A role cannot inherit from itself.");
        }

        var role = await _roleRepository.GetByIdAsync(
            command.RoleId,
            includeParentRoles: true,
            cancellationToken: cancellationToken);

        if (role is null)
        {
            throw new NotFoundException($"Role '{command.RoleId}' was not found.");
        }

        var parentRole = await _roleRepository.GetByIdAsync(
            command.ParentRoleId,
            cancellationToken: cancellationToken);

        if (parentRole is null)
        {
            throw new NotFoundException($"Parent role '{command.ParentRoleId}' was not found.");
        }

        if (role.ParentRoles.Any(existingParentRole => existingParentRole.Id == parentRole.Id))
        {
            return;
        }

        var allRoles = await _roleRepository.ListAsync(
            includeParentRoles: true,
            cancellationToken: cancellationToken);

        var roleGraph = RolePermissionResolver.BuildGraph(allRoles);
        var parentAncestors = RolePermissionResolver.ResolveAncestorRoleIds(parentRole.Id, roleGraph);
        if (parentAncestors.Contains(role.Id))
        {
            throw new ValidationException("Circular role inheritance is not allowed.");
        }

        role.AddParentRole(parentRole);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
