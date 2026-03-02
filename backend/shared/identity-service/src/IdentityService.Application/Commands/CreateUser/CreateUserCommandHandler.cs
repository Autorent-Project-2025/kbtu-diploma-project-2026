using IdentityService.Application.Constants;
using IdentityService.Application.Exceptions;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;

namespace IdentityService.Application.Commands.CreateUser;

public sealed class CreateUserCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IIdentityUnitOfWork _unitOfWork;

    public CreateUserCommandHandler(
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

    public async Task<CreateUserResult> Handle(
        CreateUserCommand command,
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

        var roleNames = command.RoleNames?
            .Where(roleName => !string.IsNullOrWhiteSpace(roleName))
            .Select(roleName => roleName.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray()
            ?? [];

        if (roleNames.Length == 0)
        {
            roleNames = [RoleConstants.User];
        }

        var roles = new List<Role>(roleNames.Length);
        foreach (var roleName in roleNames)
        {
            var role = await _roleRepository.GetByNameAsync(roleName, cancellationToken: cancellationToken);
            if (role is null)
            {
                throw new NotFoundException($"Role '{roleName}' was not found.");
            }

            roles.Add(role);
        }

        var user = new User(
            Guid.NewGuid(),
            normalizedUsername,
            normalizedEmail,
            _passwordHasher.Hash(command.Password));

        foreach (var role in roles)
        {
            user.AssignRole(role);
        }

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var assignedRoleNames = roles
            .Select(role => role.Name)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new CreateUserResult(user.Id, user.Username, user.Email, assignedRoleNames);
    }

    private static void Validate(CreateUserCommand command)
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
