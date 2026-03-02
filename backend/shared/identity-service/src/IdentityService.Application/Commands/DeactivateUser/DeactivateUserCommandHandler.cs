using IdentityService.Application.Exceptions;
using IdentityService.Application.Interfaces;

namespace IdentityService.Application.Commands.DeactivateUser;

public sealed class DeactivateUserCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IIdentityUnitOfWork _unitOfWork;

    public DeactivateUserCommandHandler(
        IUserRepository userRepository,
        IIdentityUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        DeactivateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken: cancellationToken);
        if (user is null)
        {
            throw new NotFoundException($"User '{command.UserId}' was not found.");
        }

        user.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
