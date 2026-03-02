using IdentityService.Application.Exceptions;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;

namespace IdentityService.Application.Commands.CreateRole;

public sealed class CreateRoleCommandHandler
{
    private readonly IRoleRepository _roleRepository;
    private readonly IIdentityUnitOfWork _unitOfWork;

    public CreateRoleCommandHandler(IRoleRepository roleRepository, IIdentityUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
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

        var role = new Role(Guid.NewGuid(), normalizedRoleName);
        await _roleRepository.AddAsync(role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateRoleResult(role.Id, role.Name);
    }
}
