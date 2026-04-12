using Microsoft.EntityFrameworkCore;
using TicketService.Application.Interfaces;
using TicketService.Domain.Entities;
using TicketService.Domain.Enums;

namespace TicketService.Infrastructure.Persistence.Repositories;

public sealed class AccessRequestRepository : IAccessRequestRepository
{
    private readonly TicketDbContext _context;

    public AccessRequestRepository(TicketDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(ComplaintBookingAccessRequest request, CancellationToken cancellationToken = default)
    {
        await _context.ComplaintBookingAccessRequests.AddAsync(request, cancellationToken);
    }

    public async Task<ComplaintBookingAccessRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ComplaintBookingAccessRequests
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsPendingAsync(
        Guid complaintId,
        int bookingId,
        Guid managerId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ComplaintBookingAccessRequests
            .AnyAsync(r =>
                    r.ComplaintId == complaintId &&
                    r.BookingId == bookingId &&
                    r.RequestedByManagerId == managerId &&
                    r.Status == AccessRequestStatus.Pending,
                cancellationToken);
    }

    public async Task<ComplaintBookingAccessRequest?> GetActiveGrantAsync(
        Guid managerId,
        int bookingId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ComplaintBookingAccessRequests
            .FirstOrDefaultAsync(r =>
                    r.RequestedByManagerId == managerId &&
                    r.BookingId == bookingId &&
                    r.Status == AccessRequestStatus.Approved &&
                    r.ExpiresAt != null &&
                    r.ExpiresAt > DateTime.UtcNow,
                cancellationToken);
    }

    public async Task<ComplaintBookingAccessRequest?> GetForComplaintAndManagerAsync(
        Guid complaintId,
        Guid managerId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ComplaintBookingAccessRequests
            .Where(r => r.ComplaintId == complaintId && r.RequestedByManagerId == managerId)
            .OrderByDescending(r => r.RequestedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ComplaintBookingAccessRequest>> GetAllFilteredAsync(
        AccessRequestStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query = _context.ComplaintBookingAccessRequests.AsQueryable();

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        return await query
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ComplaintBookingAccessRequest>> GetActiveGrantsByComplaintAsync(
        Guid complaintId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ComplaintBookingAccessRequests
            .Where(r =>
                r.ComplaintId == complaintId &&
                r.Status == AccessRequestStatus.Approved &&
                r.ExpiresAt != null &&
                r.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }
}
