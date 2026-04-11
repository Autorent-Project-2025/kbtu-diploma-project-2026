using TicketService.Domain.Entities;
using TicketService.Domain.Enums;

namespace TicketService.Application.Interfaces;

public interface IComplaintRepository
{
    Task AddAsync(Complaint complaint, CancellationToken cancellationToken = default);

    Task<Complaint?> GetByIdAsync(Guid complaintId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Complaint>> GetByReporterUserIdAsync(
        Guid reporterUserId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Complaint>> GetAllFilteredAsync(
        ComplaintStatus? status,
        ComplaintCategory? category,
        ComplaintPriority? priority,
        Guid? assignedToManagerId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsActiveForBookingAndReporterAsync(
        int bookingId,
        Guid reporterUserId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsForBookingAndReporterAsync(
        int bookingId,
        Guid reporterUserId,
        CancellationToken cancellationToken = default);

    Task<Complaint?> GetByBookingAndReporterAsync(
        int bookingId,
        Guid reporterUserId,
        CancellationToken cancellationToken = default);

    Task AddActionLogAsync(ComplaintActionLog actionLog, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ComplaintActionLog>> GetActionLogsAsync(
        Guid complaintId,
        CancellationToken cancellationToken = default);
}
