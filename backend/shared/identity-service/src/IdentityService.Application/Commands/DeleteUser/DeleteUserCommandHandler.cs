using IdentityService.Application.Exceptions;
using IdentityService.Application.Interfaces;

namespace IdentityService.Application.Commands.DeleteUser;

public sealed class DeleteUserCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IIdentityUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;

    public DeleteUserCommandHandler(
        IUserRepository userRepository,
        IIdentityUnitOfWork unitOfWork,
        IEventPublisher eventPublisher)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
    }

    public async Task Handle(
        DeleteUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken: cancellationToken);
        if (user is null)
        {
            throw new NotFoundException($"User '{command.UserId}' was not found.");
        }

        // Publish event before delete so consumers can act while data is still reachable
        await _eventPublisher.PublishUserDeletedAsync(command.UserId, user.Email, cancellationToken);

        _userRepository.Delete(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
