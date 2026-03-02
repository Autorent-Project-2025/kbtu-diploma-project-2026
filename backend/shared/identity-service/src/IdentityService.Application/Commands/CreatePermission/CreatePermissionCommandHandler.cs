using IdentityService.Application.Exceptions;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;

namespace IdentityService.Application.Commands.CreatePermission;

public sealed class CreatePermissionCommandHandler
{
    private readonly IPermissionRepository _permissionRepository;
    private readonly IIdentityUnitOfWork _unitOfWork;

    public CreatePermissionCommandHandler(
        IPermissionRepository permissionRepository,
        IIdentityUnitOfWork unitOfWork)
    {
        _permissionRepository = permissionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreatePermissionResult> Handle(
        CreatePermissionCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            throw new ValidationException("Permission name is required.");
        }

        var normalizedPermissionName = command.Name.Trim();
        var normalizedDescription = command.Description?.Trim() ?? string.Empty;

        if (await _permissionRepository.ExistsByNameAsync(normalizedPermissionName, cancellationToken))
        {
            throw new ConflictException("Permission already exists.");
        }

        var permission = new Permission(Guid.NewGuid(), normalizedPermissionName, normalizedDescription);
        await _permissionRepository.AddAsync(permission, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreatePermissionResult(permission.Id, permission.Name, permission.Description);
    }
}
