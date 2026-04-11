using Microsoft.EntityFrameworkCore;
using TicketService.Application.Interfaces;
using TicketService.Domain.Entities;
using TicketService.Domain.Enums;

namespace TicketService.Infrastructure.Persistence.Repositories;

public sealed class ComplaintRepository : IComplaintRepository
{
    private readonly TicketDbContext _dbContext;

    public ComplaintRepository(TicketDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Complaint complaint, CancellationToken cancellationToken = default)
    {
        await _dbContext.Complaints.AddAsync(complaint, cancellationToken);
    }

    public Task<Complaint?> GetByIdAsync(Guid complaintId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Complaints
            .Include(c => c.Attachments)
            .SingleOrDefaultAsync(c => c.Id == complaintId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Complaint>> GetByReporterUserIdAsync(
        Guid reporterUserId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Complaints
            .Include(c => c.Attachments)
            .Where(c => c.CreatedByUserId == reporterUserId)
            .OrderByDescending(c => c.CreatedAt)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Complaint>> GetAllFilteredAsync(
        ComplaintStatus? status,
        ComplaintCategory? category,
        ComplaintPriority? priority,
        Guid? assignedToManagerId,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Complaints
            .Include(c => c.Attachments)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(c => c.Status == status.Value);

        if (category.HasValue)
            query = query.Where(c => c.Category == category.Value);

        if (priority.HasValue)
            query = query.Where(c => c.Priority == priority.Value);

        if (assignedToManagerId.HasValue)
            query = query.Where(c => c.AssignedToManagerId == assignedToManagerId.Value);

        return await query
            .OrderByDescending(c => c.Priority)
            .ThenBy(c => c.CreatedAt)
            .ToArrayAsync(cancellationToken);
    }

    public Task<bool> ExistsActiveForBookingAndReporterAsync(
        int bookingId,
        Guid reporterUserId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Complaints.AnyAsync(
            c => c.BookingId == bookingId
                && c.CreatedByUserId == reporterUserId
                && c.Status != ComplaintStatus.Resolved
                && c.Status != ComplaintStatus.Rejected,
            cancellationToken);
    }
}
