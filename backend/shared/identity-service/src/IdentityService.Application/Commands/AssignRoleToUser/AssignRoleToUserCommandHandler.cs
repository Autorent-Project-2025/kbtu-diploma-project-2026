using IdentityService.Application.Exceptions;
using IdentityService.Application.Interfaces;

namespace IdentityService.Application.Commands.AssignRoleToUser;

public sealed class AssignRoleToUserCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IIdentityUnitOfWork _unitOfWork;

    public AssignRoleToUserCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IIdentityUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        AssignRoleToUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken: cancellationToken);
        if (user is null)
        {
            throw new NotFoundException($"User '{command.UserId}' was not found.");
        }

        var role = await _roleRepository.GetByIdAsync(command.RoleId, cancellationToken: cancellationToken);
        if (role is null)
        {
            throw new NotFoundException($"Role '{command.RoleId}' was not found.");
        }

        user.AssignRole(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
