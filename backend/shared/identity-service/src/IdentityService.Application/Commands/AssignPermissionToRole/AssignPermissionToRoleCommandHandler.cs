using IdentityService.Application.Exceptions;
using IdentityService.Application.Interfaces;

namespace IdentityService.Application.Commands.AssignPermissionToRole;

public sealed class AssignPermissionToRoleCommandHandler
{
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IIdentityUnitOfWork _unitOfWork;

    public AssignPermissionToRoleCommandHandler(
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        IIdentityUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        AssignPermissionToRoleCommand command,
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

        var permission = await _permissionRepository.GetByIdAsync(command.PermissionId, cancellationToken);
        if (permission is null)
        {
            throw new NotFoundException($"Permission '{command.PermissionId}' was not found.");
        }

        role.AddPermission(permission);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
