using TicketService.Domain.Entities;

namespace TicketService.Application.Interfaces;

public interface ITicketRepository
{
    Task AddAsync(Ticket ticket, CancellationToken cancellationToken = default);

    Task<Ticket?> GetByIdAsync(Guid ticketId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Ticket>> GetPendingAsync(CancellationToken cancellationToken = default);
}
