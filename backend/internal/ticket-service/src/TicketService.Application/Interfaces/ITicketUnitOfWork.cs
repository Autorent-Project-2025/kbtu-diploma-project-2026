namespace TicketService.Application.Interfaces;

public interface ITicketUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
