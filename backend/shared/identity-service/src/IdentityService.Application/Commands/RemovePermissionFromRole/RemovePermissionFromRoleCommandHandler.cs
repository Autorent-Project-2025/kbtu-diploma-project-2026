using IdentityService.Application.Exceptions;
using IdentityService.Application.Interfaces;

namespace IdentityService.Application.Commands.RemovePermissionFromRole;

public sealed class RemovePermissionFromRoleCommandHandler
{
    private readonly IRoleRepository _roleRepository;
    private readonly IIdentityUnitOfWork _unitOfWork;

    public RemovePermissionFromRoleCommandHandler(
        IRoleRepository roleRepository,
        IIdentityUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        RemovePermissionFromRoleCommand command,
        CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetByIdAsync(
            command.RoleId,
            includePermissions: true,
            cancellationToken: cancellationToken);

        if (role is null)
        {
            throw new NotFoundException($"Role '{command.RoleId}' was not found.");
        }

        if (!role.Permissions.Any(existingPermission => existingPermission.Id == command.PermissionId))
        {
            throw new NotFoundException(
                $"Permission '{command.PermissionId}' is not assigned to role '{command.RoleId}'.");
        }

        role.RemovePermission(command.PermissionId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
