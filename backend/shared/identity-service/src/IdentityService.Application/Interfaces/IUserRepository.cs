using IdentityService.Domain.Entities;

namespace IdentityService.Application.Interfaces;

public interface IUserRepository
{
    Task AddAsync(User user, CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(
        string email,
        bool includeRolesAndPermissions = false,
        CancellationToken cancellationToken = default);

    Task<User?> GetByIdAsync(
        Guid userId,
        bool includeRolesAndPermissions = false,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default);
}
