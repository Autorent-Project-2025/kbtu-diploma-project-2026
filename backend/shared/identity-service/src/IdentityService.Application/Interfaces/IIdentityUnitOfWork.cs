namespace IdentityService.Application.Interfaces;

public interface IIdentityUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
