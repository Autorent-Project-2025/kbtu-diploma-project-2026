using IdentityService.Application.Exceptions;
using IdentityService.Application.Interfaces;

namespace IdentityService.Application.Commands.RemoveParentRoleFromRole;

public sealed class RemoveParentRoleFromRoleCommandHandler
{
    private readonly IRoleRepository _roleRepository;
    private readonly IRolePermissionGraphProvider _roleGraphProvider;
    private readonly IIdentityUnitOfWork _unitOfWork;

    public RemoveParentRoleFromRoleCommandHandler(
        IRoleRepository roleRepository,
        IRolePermissionGraphProvider roleGraphProvider,
        IIdentityUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _roleGraphProvider = roleGraphProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        RemoveParentRoleFromRoleCommand command,
        CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetByIdAsync(
            command.RoleId,
            includeParentRoles: true,
            cancellationToken: cancellationToken);

        if (role is null)
        {
            throw new NotFoundException($"Role '{command.RoleId}' was not found.");
        }

        if (!role.ParentRoles.Any(existingParentRole => existingParentRole.Id == command.ParentRoleId))
        {
            throw new NotFoundException(
                $"Parent role '{command.ParentRoleId}' is not assigned to role '{command.RoleId}'.");
        }

        role.RemoveParentRole(command.ParentRoleId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _roleGraphProvider.Invalidate();
    }
}
