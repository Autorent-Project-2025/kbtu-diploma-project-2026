using IdentityService.Application.Exceptions;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;

namespace IdentityService.Application.Commands.CreateRole;

public sealed class CreateRoleCommandHandler
{
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IIdentityUnitOfWork _unitOfWork;

    public CreateRoleCommandHandler(
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        IIdentityUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateRoleResult> Handle(
        CreateRoleCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            throw new ValidationException("Role name is required.");
        }

        var normalizedRoleName = command.Name.Trim();

        if (await _roleRepository.ExistsByNameAsync(normalizedRoleName, cancellationToken))
        {
            throw new ConflictException("Role already exists.");
        }

        var permissionIds = command.PermissionIds?
            .Where(permissionId => permissionId != Guid.Empty)
            .Distinct()
            .ToArray()
            ?? [];

        var parentRoleIds = command.ParentRoleIds?
            .Where(roleId => roleId != Guid.Empty)
            .Distinct()
            .ToArray()
            ?? [];

        var role = new Role(Guid.NewGuid(), normalizedRoleName);

        foreach (var permissionId in permissionIds)
        {
            var permission = await _permissionRepository.GetByIdAsync(permissionId, cancellationToken);
            if (permission is null)
            {
                throw new NotFoundException($"Permission '{permissionId}' was not found.");
            }

            role.AddPermission(permission);
        }

        foreach (var parentRoleId in parentRoleIds)
        {
            var parentRole = await _roleRepository.GetByIdAsync(
                parentRoleId,
                cancellationToken: cancellationToken);

            if (parentRole is null)
            {
                throw new NotFoundException($"Parent role '{parentRoleId}' was not found.");
            }

            role.AddParentRole(parentRole);
        }

        await _roleRepository.AddAsync(role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateRoleResult(role.Id, role.Name);
    }
}
