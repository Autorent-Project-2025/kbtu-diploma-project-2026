using Microsoft.EntityFrameworkCore;
using TicketService.Application.Interfaces;
using TicketService.Domain.Entities;
using TicketService.Domain.Enums;

namespace TicketService.Infrastructure.Persistence.Repositories;

public sealed class ReopenRequestRepository : IReopenRequestRepository
{
    private readonly TicketDbContext _dbContext;

    public ReopenRequestRepository(TicketDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(ComplaintReopenRequest request, CancellationToken cancellationToken = default)
    {
        await _dbContext.ComplaintReopenRequests.AddAsync(request, cancellationToken);
    }

    public Task<ComplaintReopenRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.ComplaintReopenRequests
            .SingleOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<ComplaintReopenRequest>> GetByComplaintIdAsync(
        Guid complaintId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ComplaintReopenRequests
            .Where(r => r.ComplaintId == complaintId)
            .OrderByDescending(r => r.CreatedAt)
            .ToArrayAsync(cancellationToken);
    }

    public Task<bool> ExistsPendingForComplaintAsync(
        Guid complaintId, CancellationToken cancellationToken = default)
    {
        return _dbContext.ComplaintReopenRequests.AnyAsync(
            r => r.ComplaintId == complaintId && r.Status == ReopenRequestStatus.Pending,
            cancellationToken);
    }
}
