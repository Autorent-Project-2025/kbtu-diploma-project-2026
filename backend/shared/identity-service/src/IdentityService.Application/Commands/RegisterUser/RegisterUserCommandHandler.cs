using IdentityService.Application.Exceptions;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;

namespace IdentityService.Application.Commands.RegisterUser;

public sealed class RegisterUserCommandHandler
{
    private const string DefaultRoleName = "user";

    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IIdentityUnitOfWork _unitOfWork;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher,
        IIdentityUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<RegisterUserResult> Handle(
        RegisterUserCommand command,
        CancellationToken cancellationToken = default)
    {
        Validate(command);

        var normalizedEmail = command.Email.Trim().ToLowerInvariant();
        var normalizedUsername = command.Username.Trim();

        if (await _userRepository.ExistsByEmailAsync(normalizedEmail, cancellationToken))
        {
            throw new ConflictException("Email is already in use.");
        }

        if (await _userRepository.ExistsByUsernameAsync(normalizedUsername, cancellationToken))
        {
            throw new ConflictException("Username is already in use.");
        }

        var defaultRole = await _roleRepository.GetByNameAsync(DefaultRoleName, cancellationToken: cancellationToken);
        if (defaultRole is null)
        {
            throw new NotFoundException($"Default role '{DefaultRoleName}' was not found.");
        }

        var passwordHash = _passwordHasher.Hash(command.Password);
        var user = new User(Guid.NewGuid(), normalizedUsername, normalizedEmail, passwordHash);
        user.AssignRole(defaultRole);

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RegisterUserResult(user.Id, user.Username, user.Email);
    }

    private static void Validate(RegisterUserCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Username))
        {
            throw new ValidationException("Username is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Email))
        {
            throw new ValidationException("Email is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Password))
        {
            throw new ValidationException("Password is required.");
        }
    }
}
