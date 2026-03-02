using IdentityService.Application.Exceptions;
using IdentityService.Application.Interfaces;
using IdentityService.Application.Models;
using IdentityService.Domain.Entities;
using RefreshTokenEntity = IdentityService.Domain.Entities.RefreshToken;

namespace IdentityService.Application.Commands.LoginUser;

public sealed class LoginUserCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;
    private readonly IIdentityUnitOfWork _unitOfWork;

    public LoginUserCommandHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        IJwtProvider jwtProvider,
        IIdentityUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthTokensResult> Handle(
        LoginUserCommand command,
        CancellationToken cancellationToken = default)
    {
        Validate(command);

        var normalizedEmail = command.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(
            normalizedEmail,
            includeRolesAndPermissions: true,
            cancellationToken: cancellationToken);

        if (user is null || !_passwordHasher.Verify(command.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedException("User account is deactivated.");
        }

        var permissions = user.Roles
            .SelectMany(role => role.Permissions)
            .Select(permission => permission.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var accessToken = _jwtProvider.GenerateAccessToken(user.Id, user.Username, permissions);
        var refreshToken = _jwtProvider.GenerateRefreshToken();
        var nowUtc = DateTime.UtcNow;

        var refreshTokenEntity = new RefreshTokenEntity(
            Guid.NewGuid(),
            user.Id,
            _jwtProvider.ComputeRefreshTokenHash(refreshToken.Token),
            nowUtc,
            refreshToken.ExpiresAtUtc);

        await _refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthTokensResult(
            accessToken.Token,
            accessToken.ExpiresAtUtc,
            refreshToken.Token,
            refreshToken.ExpiresAtUtc);
    }

    private static void Validate(LoginUserCommand command)
    {
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
