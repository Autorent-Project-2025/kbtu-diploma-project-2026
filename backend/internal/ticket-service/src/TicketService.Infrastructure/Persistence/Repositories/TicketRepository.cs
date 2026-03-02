using Microsoft.EntityFrameworkCore;
using TicketService.Application.Interfaces;
using TicketService.Domain.Entities;
using TicketService.Domain.Enums;

namespace TicketService.Infrastructure.Persistence.Repositories;

public sealed class TicketRepository : ITicketRepository
{
    private readonly TicketDbContext _ticketDbContext;

    public TicketRepository(TicketDbContext ticketDbContext)
    {
        _ticketDbContext = ticketDbContext;
    }

    public async Task AddAsync(Ticket ticket, CancellationToken cancellationToken = default)
    {
        await _ticketDbContext.Tickets.AddAsync(ticket, cancellationToken);
    }

    public Task<Ticket?> GetByIdAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        return _ticketDbContext.Tickets.SingleOrDefaultAsync(ticket => ticket.Id == ticketId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Ticket>> GetPendingAsync(CancellationToken cancellationToken = default)
    {
        return await _ticketDbContext.Tickets
            .Where(ticket => ticket.Status == TicketStatus.Pending)
            .OrderBy(ticket => ticket.CreatedAt)
            .ToArrayAsync(cancellationToken);
    }
}
